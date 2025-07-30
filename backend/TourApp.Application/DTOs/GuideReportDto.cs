using System.Text.Json.Serialization;

namespace TourApp.Application.DTOs;

public class GuideReportDto
{
    [JsonPropertyName("tourSales")]
    public List<TourSalesInfo> TourSales { get; set; } = new();
    
    [JsonPropertyName("bestRatedTour")]
    public TourRatingInfo? BestRatedTour { get; set; }
    
    [JsonPropertyName("worstRatedTour")]
    public TourRatingInfo? WorstRatedTour { get; set; }
}

public class TourSalesInfo
{
    [JsonPropertyName("tourId")]
    public Guid TourId { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("salesCount")]
    public int SalesCount { get; set; }
}

public class TourRatingInfo
{
    [JsonPropertyName("tourId")]
    public Guid TourId { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("averageRating")]
    public double AverageRating { get; set; }
    
    [JsonPropertyName("ratingsCount")]
    public int RatingsCount { get; set; }
} 