using TourApp.Domain;

namespace TourApp.Application.DTOs;

public class RegisterTouristRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<Interest> Interests { get; set; } = new();
} 