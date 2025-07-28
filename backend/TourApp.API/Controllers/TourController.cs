using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TourApp.Application.Services;
using TourApp.Domain;
using TourApp.Infrastructure;
using System.Security.Claims;
using TourApp.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace TourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TourController : ControllerBase
{
    private readonly TourService _tourService;
    private readonly TourAppDbContext _dbContext;
    
    public TourController(TourAppDbContext dbContext)
    {
        _tourService = new TourService(dbContext);
        _dbContext = dbContext;
    }

    [HttpPost]
    [Authorize(Roles = "Guide")]
    public async Task<IActionResult> CreateTour([FromBody] CreateTourRequest request)
    {
        var guideId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (guideId == null)
            return Forbid();
        
        Console.WriteLine($"Creating tour: Name={request.Name}, Date={request.Date}, Date.Kind={request.Date.Kind}");
        
        try
        {
            var tour = await _tourService.CreateTourAsync(request, Guid.Parse(guideId));
            return Ok(tour);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating tour: {ex.Message}");
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
    public async Task<IActionResult> CancelTour([FromRoute] Guid tourId, [FromServices] EmailService emailService)
    {
        var guideId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (guideId == null)
            return Forbid();
        try
        {
            var tour = await _tourService.CancelTourAsync(tourId, Guid.Parse(guideId), emailService);
            return Ok(tour);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("purchase")]
    [Authorize(Roles = "Tourist")]
    public async Task<IActionResult> PurchaseTour([FromBody] PurchaseTourRequest request, [FromServices] EmailService emailService)
    {
        var touristId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (touristId == null)
            return Forbid();
        try
        {
            var purchase = await _tourService.PurchaseTourAsync(request, Guid.Parse(touristId), emailService);
            return Ok(purchase);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("purchase-multiple")]
    [Authorize(Roles = "Tourist")]
    public async Task<IActionResult> PurchaseMultipleTours([FromBody] PurchaseMultipleToursRequest request, [FromServices] EmailService emailService)
    {
        var touristId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (touristId == null)
            return Forbid();
        try
        {
            var purchases = new List<Purchase>();
            foreach (var tourId in request.TourIds)
            {
                var purchaseRequest = new PurchaseTourRequest { TourId = tourId, UseBonusPoints = 0 };
                var purchase = await _tourService.PurchaseTourAsync(purchaseRequest, Guid.Parse(touristId), emailService);
                purchases.Add(purchase);
            }
            return Ok(purchases);
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

    [HttpPost("report-problem")]
    [Authorize(Roles = "Tourist")]
    public async Task<IActionResult> ReportProblem([FromBody] ReportProblemRequest request, [FromServices] EmailService emailService)
    {
        var touristId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (touristId == null)
            return Forbid();
        try
        {
            var problem = await _tourService.ReportProblemAsync(request, Guid.Parse(touristId), emailService);
            return Ok(problem);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("update-problem-status")]
    [Authorize(Roles = "Guide,Admin")]
    public async Task<IActionResult> UpdateProblemStatus([FromBody] UpdateProblemStatusRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(System.Security.Claims.ClaimTypes.Role);
        if (userId == null || role == null)
            return Forbid();
        try
        {
            var userRole = (TourApp.Domain.UserRole)Enum.Parse(typeof(TourApp.Domain.UserRole), role);
            var problem = await _tourService.UpdateProblemStatusAsync(request, Guid.Parse(userId), userRole);
            return Ok(problem);
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

    [HttpGet("problems/tourist")]
    [Authorize(Roles = "Tourist")]
    public async Task<IActionResult> GetProblemsForTourist()
    {
        var touristId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (touristId == null)
            return Forbid();
        var problems = await _tourService.GetProblemsForTouristAsync(Guid.Parse(touristId));
        return Ok(problems);
    }

    [HttpGet("problems/guide")]
    [Authorize(Roles = "Guide")]
    public async Task<IActionResult> GetProblemsForGuide()
    {
        var guideId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (guideId == null)
            return Forbid();
        var problems = await _tourService.GetProblemsForGuideAsync(Guid.Parse(guideId));
        return Ok(problems);
    }

    [HttpGet("problems/admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllProblems()
    {
        var problems = await _tourService.GetAllProblemsAsync();
        return Ok(problems);
    }

    [HttpPost("create-test-tours")]
    public async Task<IActionResult> CreateTestTours()
    {
        try
        {
            var guideId = Guid.NewGuid();
            
            // Create test guide
            var guide = new User
            {
                Id = guideId,
                Username = "testguide",
                PasswordHash = "test",
                FirstName = "Test",
                LastName = "Guide",
                Email = "guide@test.com",
                Role = UserRole.Guide,
                BonusPoints = 0,
                IsMalicious = false,
                IsBlocked = false,
                AwardPoints = 0,
                IsAwardedGuide = false
            };
            
            // Create test tour
            var tour = new Tour
            {
                Id = Guid.NewGuid(),
                Name = "Beogradska tura",
                Description = "Obilazak najlepših delova Beograda",
                Difficulty = "Medium",
                Category = TourCategory.Nature,
                Price = 1500,
                Date = DateTime.UtcNow.AddDays(7),
                State = TourState.Published,
                GuideId = guideId,
                KeyPoints = new List<KeyPoint>()
            };

            // Create key points
            var keyPoints = new List<KeyPoint>
            {
                new KeyPoint
                {
                    Id = Guid.NewGuid(),
                    TourId = tour.Id,
                    Name = "Kalemegdan",
                    Description = "Istorijski centar Beograda sa prelepim pogledom na ušće Save i Dunava",
                    Latitude = 44.8235,
                    Longitude = 20.4499
                },
                new KeyPoint
                {
                    Id = Guid.NewGuid(),
                    TourId = tour.Id,
                    Name = "Skadarlija",
                    Description = "Boemska četvrt sa tradicionalnim restoranima",
                    Latitude = 44.8178,
                    Longitude = 20.4689
                },
                new KeyPoint
                {
                    Id = Guid.NewGuid(),
                    TourId = tour.Id,
                    Name = "Hram Svetog Save",
                    Description = "Najveći pravoslavni hram na Balkanu",
                    Latitude = 44.7980,
                    Longitude = 20.4689
                }
            };

            // Save to database
            _dbContext.Users.Add(guide);
            _dbContext.Tours.Add(tour);
            _dbContext.KeyPoints.AddRange(keyPoints);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Test tours created successfully", tour });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
} 