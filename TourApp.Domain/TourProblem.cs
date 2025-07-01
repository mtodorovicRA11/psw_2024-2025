namespace TourApp.Domain;

public enum ProblemStatus
{
    Pending,
    Resolved,
    UnderReview,
    Rejected
}

public class TourProblem
{
    public Guid Id { get; set; }
    public Guid TourId { get; set; }
    public Guid TouristId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProblemStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<TourProblemEvent> Events { get; set; } = new();
}

public class TourProblemEvent
{
    public Guid Id { get; set; }
    public Guid ProblemId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string? Data { get; set; }
} 