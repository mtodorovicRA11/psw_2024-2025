namespace TourApp.Application.DTOs;

public class GuideReportDto
{
    public List<TourSalesInfo> TourSales { get; set; } = new();
    public TourRatingInfo? BestRatedTour { get; set; }
    public TourRatingInfo? WorstRatedTour { get; set; }
}

public class TourSalesInfo
{
    public Guid TourId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SalesCount { get; set; }
}

public class TourRatingInfo
{
    public Guid TourId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int RatingsCount { get; set; }
} 