namespace TourApp.Application.DTOs;

using TourApp.Domain;

public class CreateTourRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public TourCategory Category { get; set; }
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public string GuideId { get; set; } = string.Empty;
} 