namespace TourApp.Application.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
} 