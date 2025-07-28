namespace TourApp.Domain;

public class Purchase
{
    public Guid Id { get; set; }
    public Guid TouristId { get; set; }
    public Guid TourId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int UsedBonusPoints { get; set; }
    public decimal FinalPrice { get; set; }
} 