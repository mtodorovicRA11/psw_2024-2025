namespace TourApp.Application.DTOs;

public class PurchaseMultipleToursRequest
{
    public List<Guid> TourIds { get; set; } = new();
} 