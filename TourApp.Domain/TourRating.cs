namespace TourApp.Domain;

public class TourRating
{
    public Guid Id { get; set; }
    public Guid TourId { get; set; }
    public Guid TouristId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public DateTime RatedAt { get; set; }
} 