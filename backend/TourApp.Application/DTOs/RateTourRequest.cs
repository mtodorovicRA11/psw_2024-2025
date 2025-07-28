namespace TourApp.Application.DTOs;

public class RateTourRequest
{
    public Guid TourId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
} 