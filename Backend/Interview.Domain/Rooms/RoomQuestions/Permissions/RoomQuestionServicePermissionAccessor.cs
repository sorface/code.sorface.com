using Interview.Domain.Permissions;
using Interview.Domain.Rooms.RoomQuestions.Records;
using Interview.Domain.Rooms.RoomQuestions.Records.Response;
using Interview.Domain.Rooms.RoomQuestions.Services;
using Interview.Domain.Rooms.RoomQuestions.Services.AnswerDetail;
using Interview.Domain.Rooms.RoomQuestions.Services.Update;
using Interview.Domain.ServiceResults.Success;

namespace Interview.Domain.Rooms.RoomQuestions.Permissions;

public class RoomQuestionServicePermissionAccessor(
    IRoomQuestionService roomQuestionService,
    ISecurityService securityService)
    : IRoomQuestionService, IServiceDecorator
{
    public async Task<RoomQuestionAnswerDetailResponse> GetAnswerDetailsAsync(RoomQuestionAnswerDetailRequest request, CancellationToken cancellationToken)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.GetRoomQuestionAnswerDetails, cancellationToken);
        return await roomQuestionService.GetAnswerDetailsAsync(request, cancellationToken);
    }

    public async Task<ServiceResult> UpdateAsync(Guid roomId, List<RoomQuestionUpdateRequest> request, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomQuestionUpdate, cancellationToken);
        return await roomQuestionService.UpdateAsync(roomId, request, cancellationToken);
    }

    public async Task<RoomQuestionDetail> ChangeActiveQuestionAsync(
        RoomQuestionChangeActiveRequest request,
        CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomQuestionChangeActiveQuestion, cancellationToken);
        return await roomQuestionService.ChangeActiveQuestionAsync(request, cancellationToken);
    }

    public async Task<RoomQuestionDetail> CreateAsync(
        RoomQuestionCreateRequest request,
        CancellationToken cancellationToken)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomQuestionCreate, cancellationToken);
        return await roomQuestionService.CreateAsync(request, cancellationToken);
    }

    public async Task<List<RoomQuestionResponse>> FindQuestionsAsync(RoomQuestionsRequest request, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomQuestionFindGuids, cancellationToken);
        return await roomQuestionService.FindQuestionsAsync(request, cancellationToken);
    }
}
