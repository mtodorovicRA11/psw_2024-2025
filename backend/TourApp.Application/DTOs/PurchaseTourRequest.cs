namespace TourApp.Application.DTOs;

public class PurchaseTourRequest
{
    public Guid TourId { get; set; }
    public int UseBonusPoints { get; set; } = 0;
} 