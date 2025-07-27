namespace TourApp.Application.DTOs;

public class ReportProblemRequest
{
    public Guid TourId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
} 