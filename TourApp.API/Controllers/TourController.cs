using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TourApp.Application.Services;
using TourApp.Domain;
using TourApp.Infrastructure;
using System.Security.Claims;

namespace TourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TourController : ControllerBase
{
    private readonly TourService _tourService;
    public TourController(TourAppDbContext dbContext)
    {
        _tourService = new TourService(dbContext);
    }

    [HttpPost]
    [Authorize(Roles = "Guide")]
    public async Task<IActionResult> CreateTour([FromBody] CreateTourRequest request)
    {
        var guideId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (guideId == null)
            return Forbid();
        try
        {
            var tour = await _tourService.CreateTourAsync(request, Guid.Parse(guideId));
            return Ok(tour);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("keypoint")]
    [Authorize(Roles = "Guide")]
    public async Task<IActionResult> AddKeyPoint([FromBody] AddKeyPointRequest request)
    {
        var guideId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (guideId == null)
            return Forbid();
        try
        {
            var keyPoint = await _tourService.AddKeyPointAsync(request, Guid.Parse(guideId));
            return Ok(keyPoint);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("publish/{tourId}")]
    [Authorize(Roles = "Guide")]
    public async Task<IActionResult> PublishTour([FromRoute] Guid tourId)
    {
        var guideId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (guideId == null)
            return Forbid();
        try
        {
            var tour = await _tourService.PublishTourAsync(tourId, Guid.Parse(guideId));
            return Ok(tour);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("cancel/{tourId}")]
    [Authorize(Roles = "Guide")]
    public async Task<IActionResult> CancelTour([FromRoute] Guid tourId)
    {
        var guideId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (guideId == null)
            return Forbid();
        try
        {
            var tour = await _tourService.CancelTourAsync(tourId, Guid.Parse(guideId));
            return Ok(tour);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("purchase")]
    [Authorize(Roles = "Tourist")]
    public async Task<IActionResult> PurchaseTour([FromBody] PurchaseTourRequest request)
    {
        var touristId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (touristId == null)
            return Forbid();
        try
        {
            var purchase = await _tourService.PurchaseTourAsync(request, Guid.Parse(touristId));
            return Ok(purchase);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("cart/add/{tourId}")]
    [Authorize(Roles = "Tourist")]
    public async Task<IActionResult> AddToCart([FromRoute] Guid tourId)
    {
        var touristId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (touristId == null)
            return Forbid();
        await _tourService.AddToCartAsync(Guid.Parse(touristId), tourId);
        return Ok();
    }

    [HttpPost("cart/remove/{tourId}")]
    [Authorize(Roles = "Tourist")]
    public async Task<IActionResult> RemoveFromCart([FromRoute] Guid tourId)
    {
        var touristId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (touristId == null)
            return Forbid();
        await _tourService.RemoveFromCartAsync(Guid.Parse(touristId), tourId);
        return Ok();
    }

    [HttpGet("cart")]
    [Authorize(Roles = "Tourist")]
    public async Task<IActionResult> GetCart()
    {
        var touristId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (touristId == null)
            return Forbid();
        var cart = await _tourService.GetCartAsync(Guid.Parse(touristId));
        return Ok(cart);
    }

    [HttpGet]
    [Authorize(Roles = "Guide")]
    public async Task<IActionResult> GetMyTours([FromQuery] TourState? state)
    {
        var guideId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (guideId == null)
            return Forbid();
        var tours = await _tourService.GetToursForGuideAsync(Guid.Parse(guideId), state);
        return Ok(tours);
    }

    [HttpGet("published")]
    [Authorize(Roles = "Tourist")]
    public async Task<IActionResult> GetPublishedTours([FromQuery] TourCategory? category, [FromQuery] Guid? guideId, [FromQuery] bool? onlyAwardedGuides)
    {
        var tours = await _tourService.GetPublishedToursAsync(category, guideId, onlyAwardedGuides);
        return Ok(tours);
    }

    [HttpPost("rate")]
    [Authorize(Roles = "Tourist")]
    public async Task<IActionResult> RateTour([FromBody] RateTourRequest request)
    {
        var touristId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (touristId == null)
            return Forbid();
        try
        {
            var rating = await _tourService.RateTourAsync(request, Guid.Parse(touristId));
            return Ok(rating);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("guide-report")]
    [Authorize(Roles = "Guide")]
    public async Task<IActionResult> GetGuideMonthlyReport([FromQuery] int year, [FromQuery] int month)
    {
        var guideId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (guideId == null)
            return Forbid();
        var report = await _tourService.GetGuideMonthlyReportAsync(Guid.Parse(guideId), year, month);
        return Ok(report);
    }
} 