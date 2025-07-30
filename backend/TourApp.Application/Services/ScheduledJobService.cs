using TourApp.Infrastructure;
using TourApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace TourApp.Application.Services;

public class ScheduledJobService
{
    private readonly TourAppDbContext _dbContext;
    private readonly EmailService _emailService;

    public ScheduledJobService(TourAppDbContext dbContext, EmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }

    /// <summary>
    /// Scheduled job that sends tour reminders 48 hours before tour starts
    /// This method is called by Hangfire scheduler
    /// </summary>
    public async Task SendTourRemindersJob()
    {
        try
        {
            var now = DateTime.UtcNow;
            var reminderTime = now.AddHours(48);
            
            // Find all tours that start in 48 hours
            var upcomingTours = await _dbContext.Tours
                .Where(t => t.State == TourState.Published && 
                           t.Date > now && 
                           t.Date <= reminderTime)
                .ToListAsync();

            foreach (var tour in upcomingTours)
            {
                // Find all tourists who purchased this tour
                var purchases = await _dbContext.Purchases
                    .Where(p => p.TourId == tour.Id)
                    .ToListAsync();

                foreach (var purchase in purchases)
                {
                    var tourist = await _dbContext.Users.FindAsync(purchase.TouristId);
                    if (tourist == null) continue;

                    var subject = $"Podsetnik: Vaša tura '{tour.Name}' se održava za 48h";
                    var body = GenerateReminderEmailBody(tourist, tour);
                    
                    await _emailService.SendEmailAsync(tourist.Email, subject, body);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw to avoid breaking the scheduled job
            Console.WriteLine($"Error in SendTourRemindersJob: {ex.Message}");
        }
    }

    /// <summary>
    /// Scheduled job that awards best guides monthly
    /// This method is called by Hangfire scheduler
    /// </summary>
    public async Task AwardBestGuidesJob()
    {
        try
        {
            var now = DateTime.UtcNow;
            var lastMonth = now.AddMonths(-1);
            
            await AwardBestGuideAsync(lastMonth.Year, lastMonth.Month);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AwardBestGuidesJob: {ex.Message}");
        }
    }

    /// <summary>
    /// Scheduled job that sends tour recommendations to interested tourists
    /// This method is called by Hangfire scheduler
    /// </summary>
    public async Task SendTourRecommendationsJob()
    {
        try
        {
            // Find recently published tours (last 24 hours)
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var newTours = await _dbContext.Tours
                .Where(t => t.State == TourState.Published && 
                           t.Date > yesterday)
                .ToListAsync();

            foreach (var tour in newTours)
            {
                await SendTourRecommendationsAsync(tour, _emailService);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendTourRecommendationsJob: {ex.Message}");
        }
    }

    private async Task AwardBestGuideAsync(int year, int month)
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

    private async Task SendTourRecommendationsAsync(Tour tour, EmailService emailService)
    {
        // Pronađi sve turiste čija interesovanja sadrže kategoriju nove ture
        // Koristimo client-side evaluaciju jer EF ne može da prevede Contains na Interests listu
        var allTourists = await _dbContext.Users
            .Where(u => u.Role == TourApp.Domain.UserRole.Tourist)
            .ToListAsync();
            
        var interestedTourists = allTourists
            .Where(u => u.Interests.Contains((TourApp.Domain.Interest)tour.Category))
            .ToList();
            
        foreach (var user in interestedTourists)
        {
            var subject = $"Preporuka: Nova tura iz oblasti vaših interesovanja - {tour.Name}";
            var body = GenerateRecommendationEmailBody(user, tour);
            await emailService.SendEmailAsync(user.Email, subject, body);
        }
    }

    private string GenerateReminderEmailBody(TourApp.Domain.User tourist, TourApp.Domain.Tour tour)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #1976d2;'>Podsetnik za vašu turu</h2>
                    <p>Poštovani {tourist.FirstName},</p>
                    <p>Podsećamo vas da ste kupili turu koja se održava za 48 sati:</p>
                    <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                        <p><strong>Naziv ture:</strong> {tour.Name}</p>
                        <p><strong>Datum:</strong> {tour.Date:dd.MM.yyyy HH:mm}</p>
                        <p><strong>Opis:</strong> {tour.Description}</p>
                        <p><strong>Cena:</strong> {tour.Price:C}</p>
                        <p><strong>Kategorija:</strong> {tour.Category}</p>
                        <p><strong>Težina:</strong> {tour.Difficulty}</p>
                    </div>
                    <p>Uživajte u vašoj turi!</p>
                    <p>TourApp tim</p>
                </div>
            </body>
            </html>";
    }

    private string GenerateRecommendationEmailBody(TourApp.Domain.User tourist, TourApp.Domain.Tour tour)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #4caf50;'>Preporuka: Nova tura iz vaših interesovanja</h2>
                    <p>Poštovani {tourist.FirstName},</p>
                    <p>Preporučujemo vam novu turu iz oblasti {tour.Category} koja možda može da vas zainteresuje:</p>
                    <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                        <p><strong>Naziv ture:</strong> {tour.Name}</p>
                        <p><strong>Opis:</strong> {tour.Description}</p>
                        <p><strong>Datum:</strong> {tour.Date:dd.MM.yyyy HH:mm}</p>
                        <p><strong>Cena:</strong> {tour.Price:C}</p>
                        <p><strong>Težina:</strong> {tour.Difficulty}</p>
                    </div>
                    <p>Pogledajte detalje i rezervišite svoje mesto!</p>
                    <p>TourApp tim</p>
                </div>
            </body>
            </html>";
    }
} 