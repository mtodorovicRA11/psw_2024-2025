using Xunit;
using TourApp.Application.DTOs;
using TourApp.Domain;
using TourApp.Application.Services;
using TourApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TourApp.Tests;

public class UserRegistrationTests
{
    private TourAppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TourAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TourAppDbContext(options);
    }

    private EmailService GetEmailService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Email:SmtpServer", "smtp.gmail.com"},
                {"Email:SmtpPort", "587"},
                {"Email:Username", "test@example.com"},
                {"Email:Password", "testpassword"},
                {"Email:FromEmail", "test@example.com"},
                {"Email:FromName", "TourApp Test"}
            })
            .Build();
        return new EmailService(configuration);
    }

    [Fact]
    public async Task RegisterTourist_ShouldCreateUserWithHashedPassword_AndUniqueUsernameEmail()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext();
        var service = new UserService(dbContext);
        var emailService = GetEmailService();
        var request = new RegisterTouristRequest
        {
            Username = "testuser",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Interests = new() { Interest.Nature, Interest.Art }
        };
        // Act
        var user = await service.RegisterTouristAsync(request, emailService);
        // Assert
        Assert.NotNull(user);
        Assert.Equal(request.Username, user.Username);
        Assert.Equal(request.Email, user.Email);
        Assert.NotEqual(request.Password, user.PasswordHash);
        Assert.Equal(UserRole.Tourist, user.Role);
        Assert.Contains(Interest.Nature, user.Interests);
        Assert.Contains(Interest.Art, user.Interests);
        // Provera unikatnosti
        await Assert.ThrowsAsync<Exception>(async () =>
            await service.RegisterTouristAsync(request, emailService));
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext();
        var service = new UserService(dbContext);
        var emailService = GetEmailService();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Jwt:Key", "test_key_12345678901234567890123456789012"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"Jwt:ExpiresInMinutes", "60"}
            })
            .Build();
        var registerRequest = new RegisterTouristRequest
        {
            Username = "loginuser",
            Password = "Pass123!",
            FirstName = "Login",
            LastName = "User",
            Email = "login@example.com",
            Interests = new() { Interest.Nature }
        };
        await service.RegisterTouristAsync(registerRequest, emailService);
        var loginRequest = new LoginRequest
        {
            Username = "loginuser",
            Password = "Pass123!"
        };
        // Act
        var response = await service.LoginAsync(loginRequest, config);
        // Assert
        Assert.NotNull(response);
        Assert.False(string.IsNullOrEmpty(response.Token));
        Assert.Equal("loginuser", response.Username);
        Assert.Equal("Tourist", response.Role);
    }
} 