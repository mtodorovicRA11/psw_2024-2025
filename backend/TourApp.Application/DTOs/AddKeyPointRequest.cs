namespace TourApp.Application.DTOs;

public class AddKeyPointRequest
{
    public Guid TourId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? ImageUrl { get; set; }
} 