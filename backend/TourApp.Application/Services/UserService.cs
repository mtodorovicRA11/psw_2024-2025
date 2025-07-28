using TourApp.Application.DTOs;
using TourApp.Domain;
using TourApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace TourApp.Application.Services;

public class UserService
{
    private readonly TourAppDbContext _dbContext;
    public UserService(TourAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User> RegisterTouristAsync(RegisterTouristRequest request, EmailService emailService)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
            throw new Exception("Username already exists");
        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
            throw new Exception("Email already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Role = UserRole.Tourist,
            Interests = request.Interests,
            BonusPoints = 0,
            IsMalicious = false,
            IsBlocked = false
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Send welcome email
        await emailService.SendWelcomeEmailAsync(user.Email, user.Username, user.FirstName);

        return user;
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, IConfiguration configuration)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || user.IsBlocked)
            throw new Exception("Invalid credentials or user is blocked");
        if (user.PasswordHash != HashPassword(request.Password))
            throw new Exception("Invalid credentials");

        var token = GenerateJwtToken(user, configuration);
        return new LoginResponse
        {
            Token = token,
            Username = user.Username,
            Role = user.Role.ToString()
        };
    }

    private string GenerateJwtToken(User user, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiresInMinutes"]!)),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task CheckAndMarkMaliciousTouristAsync(Guid touristId)
    {
        var rejectedProblems = await _dbContext.TourProblems.CountAsync(p => p.TouristId == touristId && p.Status == ProblemStatus.Rejected);
        var user = await _dbContext.Users.FindAsync(touristId);
        if (user != null && rejectedProblems >= 10)
        {
            user.IsMalicious = true;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task CheckAndMarkMaliciousGuideAsync(Guid guideId)
    {
        var cancelledTours = await _dbContext.Tours.CountAsync(t => t.GuideId == guideId && t.State == TourState.Cancelled);
        var user = await _dbContext.Users.FindAsync(guideId);
        if (user != null && cancelledTours >= 10)
        {
            user.IsMalicious = true;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<List<User>> GetMaliciousUsersAsync()
    {
        return await _dbContext.Users.Where(u => u.IsMalicious).ToListAsync();
    }

    public async Task BlockUserAsync(Guid userId, EmailService emailService)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User not found");
        user.IsBlocked = true;
        await _dbContext.SaveChangesAsync();
        await emailService.SendBlockNotificationAsync(user.Email, user.Username);
    }

    public async Task UnblockUserAsync(Guid userId, EmailService emailService)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User not found");
        user.IsBlocked = false;
        await _dbContext.SaveChangesAsync();
        await emailService.SendUnblockNotificationAsync(user.Email, user.Username);
    }
} 