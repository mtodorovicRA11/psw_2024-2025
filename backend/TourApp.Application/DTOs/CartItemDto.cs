namespace TourApp.Application.DTOs;

using TourApp.Domain;

public class CartItemDto
{
    public Guid TourId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string GuideName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TourCategory Category { get; set; }
} 