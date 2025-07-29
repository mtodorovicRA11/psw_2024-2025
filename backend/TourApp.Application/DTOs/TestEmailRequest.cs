namespace TourApp.Application.DTOs;

public class TestEmailRequest
{
    public string Email { get; set; } = string.Empty;
}

public class UpdateInterestsRequest
{
    public List<string> Interests { get; set; } = new();
} 