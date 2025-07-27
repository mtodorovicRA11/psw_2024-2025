namespace TourApp.Domain;

public enum TourCategory
{
    Nature,
    Art,
    Sport,
    Shopping,
    Food
}

public enum TourState
{
    Draft,
    Published,
    Cancelled
}

public class Tour
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public TourCategory Category { get; set; }
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public TourState State { get; set; }
    public Guid GuideId { get; set; }
    public List<KeyPoint> KeyPoints { get; set; } = new();
} 