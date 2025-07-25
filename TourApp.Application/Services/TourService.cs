using TourApp.Application.DTOs;
using TourApp.Domain;
using TourApp.Infrastructure;

namespace TourApp.Application.Services;

public class TourService
{
    private readonly TourAppDbContext _dbContext;
    public TourService(TourAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Tour> CreateTourAsync(CreateTourRequest request, Guid guideId)
    {
        var tour = new Tour
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Difficulty = request.Difficulty,
            Category = request.Category,
            Price = request.Price,
            Date = request.Date,
            State = TourState.Draft,
            GuideId = guideId,
            KeyPoints = new List<KeyPoint>()
        };
        _dbContext.Tours.Add(tour);
        await _dbContext.SaveChangesAsync();
        return tour;
    }

    public async Task<KeyPoint> AddKeyPointAsync(AddKeyPointRequest request, Guid guideId)
    {
        var tour = await _dbContext.Tours.FindAsync(request.TourId);
        if (tour == null)
            throw new Exception("Tour not found");
        if (tour.GuideId != guideId)
            throw new Exception("Not authorized to modify this tour");
        if (tour.State != TourState.Draft)
            throw new Exception("Cannot add key points to a published or cancelled tour");

        var keyPoint = new KeyPoint
        {
            Id = Guid.NewGuid(),
            TourId = request.TourId,
            Name = request.Name,
            Description = request.Description,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ImageUrl = request.ImageUrl
        };
        _dbContext.KeyPoints.Add(keyPoint);
        await _dbContext.SaveChangesAsync();
        return keyPoint;
    }

    public async Task<Tour> PublishTourAsync(Guid tourId, Guid guideId)
    {
        var tour = await _dbContext.Tours.Include(t => t.KeyPoints).FirstOrDefaultAsync(t => t.Id == tourId);
        if (tour == null)
            throw new Exception("Tour not found");
        if (tour.GuideId != guideId)
            throw new Exception("Not authorized to publish this tour");
        if (tour.State != TourState.Draft)
            throw new Exception("Only draft tours can be published");
        if (string.IsNullOrWhiteSpace(tour.Name) || string.IsNullOrWhiteSpace(tour.Description) || string.IsNullOrWhiteSpace(tour.Difficulty))
            throw new Exception("All basic information must be filled");
        if (tour.KeyPoints == null || tour.KeyPoints.Count < 2)
            throw new Exception("At least two key points are required to publish the tour");

        tour.State = TourState.Published;
        await _dbContext.SaveChangesAsync();
        return tour;
    }

    public async Task<Tour> CancelTourAsync(Guid tourId, Guid guideId)
    {
        var tour = await _dbContext.Tours.FirstOrDefaultAsync(t => t.Id == tourId);
        if (tour == null)
            throw new Exception("Tour not found");
        if (tour.GuideId != guideId)
            throw new Exception("Not authorized to cancel this tour");
        if (tour.State != TourState.Published)
            throw new Exception("Only published tours can be cancelled");
        if ((tour.Date - DateTime.UtcNow).TotalHours < 24)
            throw new Exception("Tour can only be cancelled at least 24 hours before it starts");

        // Dodela bonus poena turistima koji su kupili turu
        var purchases = _dbContext.Purchases.Where(p => p.TourId == tourId).ToList();
        foreach (var purchase in purchases)
        {
            var user = await _dbContext.Users.FindAsync(purchase.TouristId);
            if (user != null)
                user.BonusPoints += (int)tour.Price;
        }

        tour.State = TourState.Cancelled;
        await _dbContext.SaveChangesAsync();
        return tour;
    }

    public async Task<List<Tour>> GetToursForGuideAsync(Guid guideId, TourState? state = null)
    {
        var query = _dbContext.Tours.Include(t => t.KeyPoints).Where(t => t.GuideId == guideId);
        if (state.HasValue)
            query = query.Where(t => t.State == state.Value);
        return await query.ToListAsync();
    }

    public async Task<List<Tour>> GetPublishedToursAsync(TourCategory? category = null, Guid? guideId = null, bool? onlyAwardedGuides = null)
    {
        var query = _dbContext.Tours.Include(t => t.KeyPoints).Where(t => t.State == TourState.Published);
        if (category.HasValue)
            query = query.Where(t => t.Category == category.Value);
        if (guideId.HasValue)
            query = query.Where(t => t.GuideId == guideId.Value);
        if (onlyAwardedGuides == true)
        {
            var awardedGuideIds = _dbContext.Users.Where(u => u.IsAwardedGuide).Select(u => u.Id).ToList();
            query = query.Where(t => awardedGuideIds.Contains(t.GuideId));
        }
        return await query.ToListAsync();
    }

    public async Task<Purchase> PurchaseTourAsync(PurchaseTourRequest request, Guid touristId)
    {
        var tour = await _dbContext.Tours.FirstOrDefaultAsync(t => t.Id == request.TourId && t.State == TourState.Published);
        if (tour == null)
            throw new Exception("Tour not found or not available for purchase");
        var user = await _dbContext.Users.FindAsync(touristId);
        if (user == null)
            throw new Exception("User not found");
        if (user.BonusPoints < request.UseBonusPoints)
            throw new Exception("Not enough bonus points");
        var alreadyPurchased = await _dbContext.Purchases.AnyAsync(p => p.TourId == request.TourId && p.TouristId == touristId);
        if (alreadyPurchased)
            throw new Exception("Tour already purchased");

        var finalPrice = tour.Price - request.UseBonusPoints;
        if (finalPrice < 0) finalPrice = 0;
        user.BonusPoints -= request.UseBonusPoints;
        if (user.BonusPoints < 0) user.BonusPoints = 0;

        var purchase = new Purchase
        {
            Id = Guid.NewGuid(),
            TouristId = touristId,
            TourId = tour.Id,
            PurchaseDate = DateTime.UtcNow,
            UsedBonusPoints = request.UseBonusPoints,
            FinalPrice = finalPrice
        };
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();
        return purchase;
    }

    public async Task<TourRating> RateTourAsync(RateTourRequest request, Guid touristId)
    {
        var tour = await _dbContext.Tours.FindAsync(request.TourId);
        if (tour == null)
            throw new Exception("Tour not found");
        if (tour.Date > DateTime.UtcNow)
            throw new Exception("You can only rate a tour after it has taken place");
        if ((DateTime.UtcNow - tour.Date).TotalDays > 30)
            throw new Exception("You can only rate a tour within 30 days after it has taken place");
        var purchase = await _dbContext.Purchases.FirstOrDefaultAsync(p => p.TourId == request.TourId && p.TouristId == touristId);
        if (purchase == null)
            throw new Exception("You can only rate tours you have purchased");
        var alreadyRated = await _dbContext.TourRatings.AnyAsync(r => r.TourId == request.TourId && r.TouristId == touristId);
        if (alreadyRated)
            throw new Exception("You have already rated this tour");
        if (request.Rating < 1 || request.Rating > 5)
            throw new Exception("Rating must be between 1 and 5");
        if ((request.Rating == 1 || request.Rating == 2) && string.IsNullOrWhiteSpace(request.Comment))
            throw new Exception("Comment is required for ratings 1 and 2");

        var rating = new TourRating
        {
            Id = Guid.NewGuid(),
            TourId = request.TourId,
            TouristId = touristId,
            Rating = request.Rating,
            Comment = request.Comment,
            RatedAt = DateTime.UtcNow
        };
        _dbContext.TourRatings.Add(rating);
        await _dbContext.SaveChangesAsync();
        return rating;
    }

    // Korpom upravljamo u memoriji po korisniku (za demo svrhe, u realnom sistemu bi se koristila baza ili redis)
    private static readonly Dictionary<Guid, List<Guid>> _userCarts = new();

    public async Task AddToCartAsync(Guid touristId, Guid tourId)
    {
        if (!_userCarts.ContainsKey(touristId))
            _userCarts[touristId] = new List<Guid>();
        if (!_userCarts[touristId].Contains(tourId))
            _userCarts[touristId].Add(tourId);
    }

    public async Task RemoveFromCartAsync(Guid touristId, Guid tourId)
    {
        if (_userCarts.ContainsKey(touristId))
            _userCarts[touristId].Remove(tourId);
    }

    public async Task<CartDto> GetCartAsync(Guid touristId)
    {
        var result = new CartDto();
        if (_userCarts.ContainsKey(touristId))
        {
            var tourIds = _userCarts[touristId];
            var tours = await _dbContext.Tours.Include(t => t.KeyPoints).Include(t => t.GuideId).Where(t => tourIds.Contains(t.Id)).ToListAsync();
            foreach (var tour in tours)
            {
                var guide = await _dbContext.Users.FindAsync(tour.GuideId);
                result.Items.Add(new CartItemDto
                {
                    TourId = tour.Id,
                    Name = tour.Name,
                    Price = tour.Price,
                    GuideName = guide != null ? guide.FirstName + " " + guide.LastName : string.Empty,
                    Date = tour.Date,
                    Category = tour.Category
                });
            }
            result.TotalPrice = result.Items.Sum(i => i.Price);
        }
        var user = await _dbContext.Users.FindAsync(touristId);
        result.MaxUsableBonusPoints = user != null ? Math.Min(user.BonusPoints, (int)result.TotalPrice) : 0;
        return result;
    }

    public async Task SendTourRemindersAsync(EmailService emailService)
    {
        var now = DateTime.UtcNow;
        var reminderTime = now.AddHours(48);
        var purchases = await _dbContext.Purchases
            .Where(p => p.PurchaseDate < reminderTime && p.PurchaseDate > now)
            .ToListAsync();
        foreach (var purchase in purchases)
        {
            var tour = await _dbContext.Tours.FindAsync(purchase.TourId);
            if (tour == null) continue;
            var user = await _dbContext.Users.FindAsync(purchase.TouristId);
            if (user == null) continue;
            var subject = $"Podsetnik: Vaša tura '{tour.Name}' se održava za 48h";
            var body = $"Poštovani {user.FirstName},\n\nPodsećamo vas da ste kupili turu '{tour.Name}' koja se održava {tour.Date}.\n\nOpis: {tour.Description}\nCena: {tour.Price}\nVodič: {tour.GuideId}";
            await emailService.SendEmailAsync(user.Email, subject, body);
        }
    }

    public async Task<GuideReportDto> GetGuideMonthlyReportAsync(Guid guideId, int year, int month)
    {
        var tours = await _dbContext.Tours.Where(t => t.GuideId == guideId && t.Date.Year == year && t.Date.Month == month).ToListAsync();
        var tourIds = tours.Select(t => t.Id).ToList();
        var purchases = await _dbContext.Purchases.Where(p => tourIds.Contains(p.TourId) && p.PurchaseDate.Year == year && p.PurchaseDate.Month == month).ToListAsync();
        var ratings = await _dbContext.TourRatings.Where(r => tourIds.Contains(r.TourId)).ToListAsync();

        var report = new GuideReportDto();
        foreach (var tour in tours)
        {
            var salesCount = purchases.Count(p => p.TourId == tour.Id);
            report.TourSales.Add(new TourSalesInfo
            {
                TourId = tour.Id,
                Name = tour.Name,
                SalesCount = salesCount
            });
        }
        var ratedTours = tours.Select(t => new
        {
            Tour = t,
            Ratings = ratings.Where(r => r.TourId == t.Id).ToList()
        }).Where(x => x.Ratings.Any()).ToList();
        if (ratedTours.Any())
        {
            var best = ratedTours.OrderByDescending(x => x.Ratings.Average(r => r.Rating)).First();
            report.BestRatedTour = new TourRatingInfo
            {
                TourId = best.Tour.Id,
                Name = best.Tour.Name,
                AverageRating = best.Ratings.Average(r => r.Rating),
                RatingsCount = best.Ratings.Count
            };
            var worst = ratedTours.OrderBy(x => x.Ratings.Average(r => r.Rating)).First();
            report.WorstRatedTour = new TourRatingInfo
            {
                TourId = worst.Tour.Id,
                Name = worst.Tour.Name,
                AverageRating = worst.Ratings.Average(r => r.Rating),
                RatingsCount = worst.Ratings.Count
            };
        }
        return report;
    }

    public async Task AwardBestGuideAsync(int year, int month)
    {
        // Pronađi sve ture održane u datom mesecu
        var tours = await _dbContext.Tours.Where(t => t.Date.Year == year && t.Date.Month == month).ToListAsync();
        var tourIds = tours.Select(t => t.Id).ToList();
        var purchases = await _dbContext.Purchases.Where(p => tourIds.Contains(p.TourId)).ToListAsync();
        // Grupisanje po vodiču
        var guideSales = tours.GroupBy(t => t.GuideId)
            .Select(g => new { GuideId = g.Key, Sales = purchases.Count(p => g.Select(t => t.Id).Contains(p.TourId)) })
            .OrderByDescending(x => x.Sales)
            .ToList();
        if (!guideSales.Any() || guideSales.First().Sales == 0)
            return;
        var bestGuideId = guideSales.First().GuideId;
        var bestGuide = await _dbContext.Users.FindAsync(bestGuideId);
        if (bestGuide != null)
        {
            bestGuide.AwardPoints += 1;
            if (bestGuide.AwardPoints >= 5)
                bestGuide.IsAwardedGuide = true;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task SendTourRecommendationsAsync(Tour tour, EmailService emailService)
    {
        // Pronađi sve turiste čija interesovanja sadrže kategoriju nove ture
        var interestedTourists = await _dbContext.Users
            .Where(u => u.Role == UserRole.Tourist && u.Interests.Contains((Interest)tour.Category))
            .ToListAsync();
        foreach (var user in interestedTourists)
        {
            var subject = $"Preporuka: Nova tura iz oblasti vaših interesovanja - {tour.Name}";
            var body = $"Poštovani {user.FirstName},\n\nPreporučujemo vam novu turu '{tour.Name}' iz oblasti {tour.Category}.\n\nOpis: {tour.Description}\nCena: {tour.Price}\nDatum: {tour.Date}";
            await emailService.SendEmailAsync(user.Email, subject, body);
        }
    }
} 