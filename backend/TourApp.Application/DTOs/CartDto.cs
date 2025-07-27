namespace TourApp.Application.DTOs;

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalPrice { get; set; }
    public int MaxUsableBonusPoints { get; set; }
} 