namespace TourApp.Domain;

public class KeyPoint
{
    public Guid Id { get; set; }
    public Guid TourId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? ImageUrl { get; set; }
} 