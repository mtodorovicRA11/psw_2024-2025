using TourApp.Domain;
using TourApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace TourApp.Application.Services;

public interface IProblemEventStore
{
    Task SaveEventAsync(ProblemEvent problemEvent);
    Task<List<ProblemEvent>> GetEventsForProblemAsync(Guid problemId);
    Task<List<ProblemEvent>> GetAllEventsAsync();
}

public class ProblemEventStore : IProblemEventStore
{
    private readonly TourAppDbContext _dbContext;

    public ProblemEventStore(TourAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveEventAsync(ProblemEvent problemEvent)
    {
        var eventEntity = new ProblemEventEntity
        {
            Id = problemEvent.Id,
            ProblemId = problemEvent.ProblemId,
            EventType = problemEvent.GetType().Name,
            OccurredAt = problemEvent.OccurredAt,
            UserId = problemEvent.UserId,
            UserRole = problemEvent.UserRole.ToString(),
            OldStatus = problemEvent.OldStatus.ToString(),
            NewStatus = problemEvent.NewStatus.ToString(),
            Comment = problemEvent.Comment,
            EventData = System.Text.Json.JsonSerializer.Serialize(problemEvent)
        };

        _dbContext.ProblemEvents.Add(eventEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<ProblemEvent>> GetEventsForProblemAsync(Guid problemId)
    {
        var events = await _dbContext.ProblemEvents
            .Where(e => e.ProblemId == problemId)
            .OrderBy(e => e.OccurredAt)
            .ToListAsync();

        return events.Select(e => DeserializeEvent(e)).ToList();
    }

    public async Task<List<ProblemEvent>> GetAllEventsAsync()
    {
        var events = await _dbContext.ProblemEvents
            .OrderBy(e => e.OccurredAt)
            .ToListAsync();

        return events.Select(e => DeserializeEvent(e)).ToList();
    }

    private ProblemEvent DeserializeEvent(ProblemEventEntity eventEntity)
    {
        try
        {
            return eventEntity.EventType switch
            {
                nameof(ProblemCreatedEvent) => System.Text.Json.JsonSerializer.Deserialize<ProblemCreatedEvent>(eventEntity.EventData)!,
                nameof(ProblemStatusChangedEvent) => System.Text.Json.JsonSerializer.Deserialize<ProblemStatusChangedEvent>(eventEntity.EventData)!,
                _ => throw new Exception($"Unknown event type: {eventEntity.EventType}")
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deserializing event {eventEntity.Id}: {ex.Message}");
        }
    }
} 