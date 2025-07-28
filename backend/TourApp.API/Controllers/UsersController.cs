using Microsoft.AspNetCore.Mvc;
using TourApp.Application.DTOs;
using TourApp.Application.Services;
using TourApp.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace TourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    public UsersController(TourAppDbContext dbContext)
    {
        _userService = new UserService(dbContext);
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
} 