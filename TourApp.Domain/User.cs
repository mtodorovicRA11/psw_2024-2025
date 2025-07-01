namespace TourApp.Domain;

public enum UserRole
{
    Tourist,
    Guide,
    Admin
}

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public List<Interest> Interests { get; set; } = new();
    public int BonusPoints { get; set; }
    public bool IsMalicious { get; set; }
    public bool IsBlocked { get; set; }
}

public enum Interest
{
    Nature,
    Art,
    Sport,
    Shopping,
    Food
} 