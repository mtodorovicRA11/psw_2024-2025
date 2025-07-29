using Xunit;
using TourApp.Application.DTOs;
using TourApp.Domain;
using TourApp.Application.Services;
using TourApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Moq;

namespace TourApp.Tests;

public class AdminAndReportTests
{
    private TourAppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TourAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TourAppDbContext(options);
    }

    [Fact]
    public async Task BlockAndUnblockUser_ShouldChangeBlockedStatus()
    {
        var dbContext = GetInMemoryDbContext();
        var userService = new UserService(dbContext);
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
        var emailService = new EmailService(configuration);
        var userId = Guid.NewGuid();
        dbContext.Users.Add(new User { Id = userId, Username = "admin", PasswordHash = "x", Role = UserRole.Tourist, Email = "a@a.com" });
        await dbContext.SaveChangesAsync();
        await userService.BlockUserAsync(userId, emailService);
        var user = await dbContext.Users.FindAsync(userId);
        Assert.True(user.IsBlocked);
        await userService.UnblockUserAsync(userId, emailService);
        user = await dbContext.Users.FindAsync(userId);
        Assert.False(user.IsBlocked);
    }

    [Fact]
    public async Task MarkMaliciousTourist_ShouldSetIsMalicious()
    {
        var dbContext = GetInMemoryDbContext();
        var userService = new UserService(dbContext);
        var touristId = Guid.NewGuid();
        dbContext.Users.Add(new User { Id = touristId, Username = "t", PasswordHash = "x", Role = UserRole.Tourist, Email = "t@t.com" });
        for (int i = 0; i < 10; i++)
        {
            dbContext.TourProblems.Add(new TourProblem { Id = Guid.NewGuid(), TouristId = touristId, TourId = Guid.NewGuid(), Title = "P", Description = "D", Status = ProblemStatus.Rejected, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        }
        await dbContext.SaveChangesAsync();
        await userService.CheckAndMarkMaliciousTouristAsync(touristId);
        var user = await dbContext.Users.FindAsync(touristId);
        Assert.True(user.IsMalicious);
    }

    [Fact]
    public async Task MarkMaliciousGuide_ShouldSetIsMalicious()
    {
        var dbContext = GetInMemoryDbContext();
        var userService = new UserService(dbContext);
        var guideId = Guid.NewGuid();
        dbContext.Users.Add(new User { Id = guideId, Username = "g", PasswordHash = "x", Role = UserRole.Guide, Email = "g@g.com" });
        for (int i = 0; i < 10; i++)
        {
            dbContext.Tours.Add(new Tour { Id = Guid.NewGuid(), GuideId = guideId, Name = "T", Description = "D", Difficulty = "E", Category = TourCategory.Nature, Price = 10, Date = DateTime.UtcNow, State = TourState.Cancelled });
        }
        await dbContext.SaveChangesAsync();
        await userService.CheckAndMarkMaliciousGuideAsync(guideId);
        var user = await dbContext.Users.FindAsync(guideId);
        Assert.True(user.IsMalicious);
    }

    [Fact]
    public async Task AwardBestGuide_ShouldGiveAwardPointsAndSetAwardedStatus()
    {
        var dbContext = GetInMemoryDbContext();
        var eventStore = new Mock<IProblemEventStore>().Object;
        var userService = new UserService(dbContext);
        var service = new TourService(dbContext, eventStore, userService);
        var guideId = Guid.NewGuid();
        dbContext.Users.Add(new User { Id = guideId, Username = "g", PasswordHash = "x", Role = UserRole.Guide, Email = "g@g.com", AwardPoints = 4 });
        for (int i = 0; i < 5; i++)
        {
            var tourId = Guid.NewGuid();
            dbContext.Tours.Add(new Tour { Id = tourId, GuideId = guideId, Name = "T", Description = "D", Difficulty = "E", Category = TourCategory.Nature, Price = 10, Date = new DateTime(2024, 6, 1), State = TourState.Published });
            dbContext.Purchases.Add(new Purchase { Id = Guid.NewGuid(), TouristId = Guid.NewGuid(), TourId = tourId, PurchaseDate = new DateTime(2024, 6, 1), UsedBonusPoints = 0, FinalPrice = 10 });
        }
        await dbContext.SaveChangesAsync();
        await service.AwardBestGuideAsync(2024, 6);
        var guide = await dbContext.Users.FindAsync(guideId);
        Assert.Equal(5, guide.AwardPoints);
        Assert.True(guide.IsAwardedGuide);
    }

    [Fact]
    public async Task GetGuideMonthlyReport_ShouldReturnSalesAndRatings()
    {
        var dbContext = GetInMemoryDbContext();
        var eventStore = new Mock<IProblemEventStore>().Object;
        var userService = new UserService(dbContext);
        var service = new TourService(dbContext, eventStore, userService);
        var guideId = Guid.NewGuid();
        var tourId = Guid.NewGuid();
        dbContext.Tours.Add(new Tour { Id = tourId, GuideId = guideId, Name = "T", Description = "D", Difficulty = "E", Category = TourCategory.Nature, Price = 10, Date = new DateTime(2024, 6, 1), State = TourState.Published });
        dbContext.Purchases.Add(new Purchase { Id = Guid.NewGuid(), TouristId = Guid.NewGuid(), TourId = tourId, PurchaseDate = new DateTime(2024, 6, 1), UsedBonusPoints = 0, FinalPrice = 10 });
        dbContext.TourRatings.Add(new TourRating { Id = Guid.NewGuid(), TourId = tourId, TouristId = Guid.NewGuid(), Rating = 5, Comment = "Odlično", RatedAt = DateTime.UtcNow });
        await dbContext.SaveChangesAsync();
        var report = await service.GetGuideMonthlyReportAsync(guideId, 2024, 6);
        Assert.Single(report.TourSales);
        Assert.NotNull(report.BestRatedTour);
        Assert.Equal(5, report.BestRatedTour.AverageRating);
    }
} 