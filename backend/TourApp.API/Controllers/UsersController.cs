using Microsoft.AspNetCore.Mvc;
using TourApp.Application.DTOs;
using TourApp.Application.Services;
using TourApp.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using TourApp.Domain;
using System.Security.Claims;

namespace TourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly TourAppDbContext _dbContext;
    public UsersController(TourAppDbContext dbContext)
    {
        _userService = new UserService(dbContext);
        _dbContext = dbContext;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterTouristRequest request, [FromServices] EmailService emailService)
    {
        try
        {
            var user = await _userService.RegisterTouristAsync(request, emailService);
            return Ok(new { user.Id, user.Username, user.Email, user.FirstName, user.LastName, user.Interests });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromServices] IConfiguration configuration)
    {
        try
        {
            var response = await _userService.LoginAsync(request, configuration);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("malicious")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetMaliciousUsers()
    {
        var users = await _userService.GetMaliciousUsersAsync();
        return Ok(users);
    }

    [HttpPost("block/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BlockUser([FromRoute] Guid userId, [FromServices] EmailService emailService)
    {
        try
        {
            await _userService.BlockUserAsync(userId, emailService);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("unblock/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnblockUser([FromRoute] Guid userId, [FromServices] EmailService emailService)
    {
        try
        {
            await _userService.UnblockUserAsync(userId, emailService);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request, [FromServices] EmailService emailService)
    {
        try
        {
            await emailService.SendTestEmailAsync(request.Email);
            return Ok(new { message = "Test email sent successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("update-interests")]
    [Authorize(Roles = "Tourist")]
    public async Task<IActionResult> UpdateInterests([FromBody] UpdateInterestsRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Forbid();
        
        try
        {
            await _userService.UpdateInterestsAsync(Guid.Parse(userId), request.Interests);
            return Ok(new { message = "Interests updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("create-malicious-users")]
    public async Task<IActionResult> CreateMaliciousUsers()
    {
        try
        {
            // Kreiraj test korisnike koji su maliciozni
            var maliciousTourist = new User
            {
                Id = Guid.NewGuid(),
                Username = "malicious_tourist",
                Email = "malicious.tourist@test.com",
                FirstName = "Maliciozni",
                LastName = "Turista",
                PasswordHash = "test_hash",
                Role = UserRole.Tourist,
                IsMalicious = true,
                IsBlocked = false,
                Interests = new List<Interest> { Interest.Nature },
                BonusPoints = 0
            };

            var maliciousGuide = new User
            {
                Id = Guid.NewGuid(),
                Username = "malicious_guide",
                Email = "malicious.guide@test.com",
                FirstName = "Maliciozni",
                LastName = "Vodič",
                PasswordHash = "test_hash",
                Role = UserRole.Guide,
                IsMalicious = true,
                IsBlocked = false,
                Interests = new List<Interest> { Interest.Nature },
                BonusPoints = 0
            };

            var blockedUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "blocked_user",
                Email = "blocked.user@test.com",
                FirstName = "Blokirani",
                LastName = "Korisnik",
                PasswordHash = "test_hash",
                Role = UserRole.Tourist,
                IsMalicious = false,
                IsBlocked = true,
                Interests = new List<Interest> { Interest.Art },
                BonusPoints = 0
            };

            _dbContext.Users.Add(maliciousTourist);
            _dbContext.Users.Add(maliciousGuide);
            _dbContext.Users.Add(blockedUser);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Malicious users created successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
} 