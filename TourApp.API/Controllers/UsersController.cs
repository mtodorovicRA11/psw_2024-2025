using Microsoft.AspNetCore.Mvc;
using TourApp.Application.DTOs;
using TourApp.Application.Services;
using TourApp.Infrastructure;
using Microsoft.Extensions.Configuration;

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
    public async Task<IActionResult> Register([FromBody] RegisterTouristRequest request)
    {
        try
        {
            var user = await _userService.RegisterTouristAsync(request);
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
} 