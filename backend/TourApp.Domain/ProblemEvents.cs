using System.Text.Json.Serialization;

namespace TourApp.Domain;

public abstract class ProblemEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProblemId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public UserRole UserRole { get; set; }
    public ProblemStatus OldStatus { get; set; }
    public ProblemStatus NewStatus { get; set; }
    public string? Comment { get; set; }
}

public class ProblemStatusChangedEvent : ProblemEvent
{
    [JsonConstructor]
    public ProblemStatusChangedEvent(Guid problemId, string userId, UserRole userRole, ProblemStatus oldStatus, ProblemStatus newStatus, string? comment = null)
    {
        ProblemId = problemId;
        UserId = userId;
        UserRole = userRole;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        Comment = comment;
    }
}

public class ProblemCreatedEvent : ProblemEvent
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid TourId { get; set; }
    public Guid TouristId { get; set; }

    [JsonConstructor]
    public ProblemCreatedEvent(Guid problemId, Guid touristId, Guid tourId, string title, string description)
    {
        ProblemId = problemId;
        TouristId = touristId;
        TourId = tourId;
        Title = title;
        Description = description;
        OldStatus = ProblemStatus.Pending; // Initial status
        NewStatus = ProblemStatus.Pending;
        UserRole = UserRole.Tourist;
    }
} 