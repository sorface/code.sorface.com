using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.EventProvider;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomQuestionPostProcessor(
    IRoomEventDispatcher eventDispatcher,
    AppDbContext db,
    RoomCodeEditorChangeEventHandler roomCodeEditorChangeEventHandler,
    ICurrentUserAccessor currentUserAccessor,
    IRoomConfigurationRepository roomConfigurationRepository,
    ISystemClock clock,
    RoomEventProviderFactory roomEventProviderFactory,
    IRoomEventDeserializer eventDeserializer,
    ILogger<RoomQuestionPostProcessor> logger)
    : EntityPostProcessor<RoomQuestion>
{
    public override async ValueTask ProcessAddedAsync(RoomQuestion entity, CancellationToken cancellationToken)
    {
        var @event = new RoomQuestionAddEvent
        {
            RoomId = entity.RoomId,
            Value = new RoomQuestionAddEventPayload(entity.QuestionId, entity.State.EnumValue),
            CreatedById = currentUserAccessor.GetUserIdOrThrow(),
        };

        await eventDispatcher.WriteAsync(@event, cancellationToken);
    }

    public override async ValueTask ProcessModifiedAsync(
        RoomQuestion original,
        RoomQuestion current,
        CancellationToken cancellationToken)
    {
        if (original.State == current.State)
        {
            return;
        }

        var @event = new RoomQuestionChangeEvent
        {
            RoomId = current.RoomId,
            Value = new RoomQuestionChangeEventPayload(current.QuestionId, original.State.EnumValue, current.State.EnumValue),
            CreatedById = currentUserAccessor.GetUserIdOrThrow(),
        };

        await eventDispatcher.WriteAsync(@event, cancellationToken);

        if (current.State == RoomQuestionState.Active)
        {
            await ChangeCodeEditorEnabledStateAsync(current, cancellationToken);

            // Changed active question
            if (original.State != RoomQuestionState.Active)
            {
                await ChangeCodeEditorContentAsync(current, cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ChangeCodeEditorEnabledStateAsync(RoomQuestion current, CancellationToken cancellationToken)
    {
        var codeEditorPayload = await GetCodeEditorEventPayload(current, cancellationToken);
        var request = new RoomCodeEditorChangeEventHandler.Request(current.RoomId, codeEditorPayload.Enabled, EVRoomCodeEditorChangeSource.System)
        {
            SaveChanges = false,
        };
        await roomCodeEditorChangeEventHandler.HandleAsync(request, cancellationToken);
    }

    private async Task ChangeCodeEditorContentAsync(RoomQuestion current, CancellationToken cancellationToken)
    {
        var codeEditorContent = await GetCodeEditorContentAsync(current, cancellationToken);
        var upsertCodeStateRequest = new UpsertCodeStateRequest
        {
            RoomId = current.RoomId,
            CodeEditorContent = codeEditorContent ?? string.Empty,
            ChangeCodeEditorContentSource = EVRoomCodeEditorChangeSource.System,
            SaveChanges = false,
        };
        await roomConfigurationRepository.UpsertCodeStateAsync(upsertCodeStateRequest, cancellationToken);
    }

    private async ValueTask<string?> GetCodeEditorContentAsync(RoomQuestion current, CancellationToken cancellationToken)
    {
        var eventProvider = await roomEventProviderFactory.CreateProviderAsync(current.RoomId, cancellationToken);
        var facade = new RoomEventActiveQuestionProvider(eventProvider, eventDeserializer, clock);
        var lastActiveQuestionTime = await facade
            .GetActiveQuestionDateAsync(current.QuestionId, cancellationToken)
            .OrderByDescending(e => e.StartActiveDate)
            .Select(e => ((DateTime StartActiveDate, DateTime EndActiveDate)?)e)
            .FirstOrDefaultAsync(cancellationToken);
        if (lastActiveQuestionTime is null)
        {
            logger.LogTrace("Not found last active question time {QuestionId}", current.QuestionId);
        }
        else
        {
            logger.LogTrace("Found last active question time {QuestionId} {StartTime} {EndTime}", current.QuestionId, lastActiveQuestionTime?.StartActiveDate, lastActiveQuestionTime?.EndActiveDate);
        }

        if (lastActiveQuestionTime is not null)
        {
            var codeEditorContentRequest = new EPStorageEventRequest
            {
                Type = EventType.CodeEditorChange,
                From = lastActiveQuestionTime.Value.StartActiveDate,
                To = lastActiveQuestionTime.Value.EndActiveDate,
            };
            var lastCodeEditorState = await eventProvider.GetLatestEventAsync(codeEditorContentRequest, cancellationToken);
            if (lastCodeEditorState is null)
            {
                logger.LogTrace("Not found code editor content for {QuestionId}", current.QuestionId);
            }
            else
            {
                logger.LogTrace("Found code editor content for {QuestionId} {Content}", current.QuestionId, lastCodeEditorState.Payload);
            }

            if (lastCodeEditorState is not null)
            {
                return lastCodeEditorState.Payload;
            }
        }

        if (current.Question is not null && current.Question.CodeEditorId is null)
        {
            return null;
        }

        var content = current.Question?.CodeEditor?.Content;
        if (content is not null)
        {
            return content;
        }

        var resultContent = await db.Questions
            .AsNoTracking()
            .Include(e => e.CodeEditor)
            .Where(e => e.Id == current.QuestionId)
            .Select(e => e.CodeEditor == null ? null : e.CodeEditor.Content)
            .FirstOrDefaultAsync(cancellationToken);
        return resultContent;
    }

    private async Task<RoomCodeEditorEnabledEvent.Payload> GetCodeEditorEventPayload(RoomQuestion roomQuestion, CancellationToken cancellationToken)
    {
        if (roomQuestion.Question is not null)
        {
            return new RoomCodeEditorEnabledEvent.Payload { Enabled = roomQuestion.Question.CodeEditorId is not null, };
        }

        var hasCodeEditor = await db.Questions
            .Where(e => e.Id == roomQuestion.QuestionId)
            .AnyAsync(e => e.CodeEditorId != null, cancellationToken);
        return new RoomCodeEditorEnabledEvent.Payload { Enabled = hasCodeEditor, };
    }
}
