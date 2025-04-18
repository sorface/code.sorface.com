using Interview.Domain.Database;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Rooms.RoomReviews;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Rooms.Service;

public sealed class RoomAnalyticService(AppDbContext db) : ISelfScopeService
{
    public async Task<Analytics> GetAsync(RoomAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        var analytics = await GetAnalyticsCoreAsync(request.RoomId, cancellationToken);
        if (analytics == null)
        {
            throw NotFoundException.Create<Room>(request.RoomId);
        }

        foreach (var analyticsQuestion in analytics.Questions!)
        {
            analyticsQuestion.Users = await GetUsersByQuestionIdAsync(request.RoomId, analyticsQuestion.Id, cancellationToken);

            if (analytics.Completed)
            {
                analyticsQuestion.AverageMark = analyticsQuestion.Users
                    .Where(e => e.Evaluation?.Mark is not null && e.Evaluation.Mark.Value > 0)
                    .Select(e => e.Evaluation!.Mark!.Value)
                    .DefaultIfEmpty(0)
                    .Average();
            }
        }

        return analytics;
    }

    private async Task<Analytics?> GetAnalyticsCoreAsync(Guid roomId, CancellationToken ct)
    {
        var res = await db.Rooms.AsNoTracking()
            .Include(e => e.Questions).ThenInclude(e => e.Question)
            .Include(e => e.Participants)
            .Where(e => e.Id == roomId)
            .Select(e => new ServiceAnalytic
            {
                Questions = e.Questions
                    .OrderBy(rq => rq.Order)
                    .Select(q => new Analytics.AnalyticsQuestion
                    {
                        Id = q.Question!.Id,
                        Status = q.State!.Name,
                        Value = q.Question.Value,
                        Users = null,
                        AverageMark = null,
                    })
                    .ToList(),
                AverageMark = null,
                UserReview = new List<Analytics.AnalyticsUserAverageMark>(),
                Completed = false,
                IsAiRoom = e.Type == SERoomType.AI,
                OpenedQuestions = e.Questions
                    .Where(e => e.State == RoomQuestionState.Open)
                    .Select(e => e.Question!.Id).ToList(),
            })
            .FirstOrDefaultAsync(ct);
        if (res is null)
        {
            return null;
        }

        // https://github.com/sorface/interview/issues/620
        if (res is { IsAiRoom: true, Questions.Count: > 0 })
        {
            res.Questions.RemoveAll(e => res.OpenedQuestions.Contains(e.Id));
        }

        var userReview = await db.RoomParticipants.AsNoTracking()
            .Include(e => e.Room)
            .Include(e => e.User)
            .ThenInclude(e => e.RoomQuestionEvaluations.Where(rqe => rqe.RoomQuestion!.RoomId == roomId))
            .Include(participant => participant.Review)
            .Where(e => e.Room.Id == roomId)
            .Select(e => new
            {
                UserId = e.User.Id,
                AverageMarks = e.Review == null || e.Review.State != SERoomReviewState.Closed
                    ? null
                    : e.User.RoomQuestionEvaluations
                        .Where(rqe => rqe.RoomQuestion!.RoomId == roomId && rqe.Mark != null && rqe.Mark > 0)
                        .Select(rqe => rqe.Mark ?? 0)
                        .ToList(),
                Comment = e.Review == null || e.Review.State != SERoomReviewState.Closed
                    ? null
                    : e.Review!.Review,
                Nickname = e.User.Nickname,
                Avatar = e.User.Avatar,
                ParticipantType = e.Type,
                Incomplete = e.User.RoomQuestionEvaluations
                    .Any(rqe => rqe.RoomQuestion!.RoomId == roomId && rqe.State == SERoomQuestionEvaluationState.Draft),
            })
            .ToListAsync(ct);
        res.UserReview = userReview
            .Select(e => new Analytics.AnalyticsUserAverageMark
            {
                UserId = e.UserId,
                AverageMark = e.AverageMarks?.DefaultIfEmpty(0).Average(),
                Comment = string.IsNullOrWhiteSpace(e.Comment) ? null : e.Comment,
                Nickname = e.Nickname,
                Avatar = e.Avatar,
                ParticipantType = e.ParticipantType.EnumValue,
            })
            .ToList();
        res.Completed = userReview.All(e => !e.Incomplete);
        if (res.Completed)
        {
            res.AverageMark = res.UserReview.All(e => e.AverageMark is null)
                ? null
                : res.UserReview.Select(e => e.AverageMark)
                    .Where(e => e > 0)
                    .DefaultIfEmpty(0)
                    .Average();
        }

        return res;
    }

    private Task<List<Analytics.AnalyticsUser>> GetUsersByQuestionIdAsync(Guid roomId, Guid questionId, CancellationToken ct)
    {
        return db.RoomParticipants.AsNoTracking()
            .Include(e => e.Room)
            .Include(e => e.User).ThenInclude(e =>
                e.RoomQuestionEvaluations.Where(rqe => rqe.RoomQuestion!.RoomId == roomId && rqe.RoomQuestion!.QuestionId == questionId))
            .Where(e => e.Room.Id == roomId)
            .Select(e => new Analytics.AnalyticsUser
            {
                Id = e.User.Id,
                Evaluation = e.User.RoomQuestionEvaluations
                    .Where(rqe => rqe.RoomQuestion!.RoomId == roomId && rqe.RoomQuestion!.QuestionId == questionId &&
                                  rqe.State == SERoomQuestionEvaluationState.Submitted)
                    .Select(rqe => new Analytics.AnalyticsUserQuestionEvaluation { Review = rqe.Review, Mark = rqe.Mark, })
                    .FirstOrDefault(),
            })
            .ToListAsync(ct);
    }

    private sealed class ServiceAnalytic : Analytics
    {
        public required bool IsAiRoom { get; set; }

        public required List<Guid> OpenedQuestions { get; set; }
    }
}
