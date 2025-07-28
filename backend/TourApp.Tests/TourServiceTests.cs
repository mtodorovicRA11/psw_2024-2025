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

namespace TourApp.Tests;

public class TourServiceTests
{
    private TourAppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TourAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TourAppDbContext(options);
    }

    [Fact]
    public async Task CreateTour_ShouldCreateDraftTour_ForGuide()
    {
        var dbContext = GetInMemoryDbContext();
        var service = new TourService(dbContext);
        var guideId = Guid.NewGuid();
        var request = new CreateTourRequest
        {
            Name = "Test Tour",
            Description = "Test Desc",
            Difficulty = "Easy",
            Category = TourCategory.Nature,
            Price = 100,
            Date = DateTime.UtcNow.AddDays(10)
        };
        var tour = await service.CreateTourAsync(request, guideId);
        Assert.NotNull(tour);
        Assert.Equal(TourState.Draft, tour.State);
        Assert.Equal(guideId, tour.GuideId);
    }

    [Fact]
    public async Task AddKeyPoint_ShouldAddKeyPointToDraftTour()
    {
        var dbContext = GetInMemoryDbContext();
        var service = new TourService(dbContext);
        var guideId = Guid.NewGuid();
        var tour = await service.CreateTourAsync(new CreateTourRequest
        {
            Name = "Tour",
            Description = "Desc",
            Difficulty = "Medium",
            Category = TourCategory.Art,
            Price = 50,
            Date = DateTime.UtcNow.AddDays(5)
        }, guideId);
        var keyPointRequest = new AddKeyPointRequest
        {
            TourId = tour.Id,
            Name = "Museum",
            Description = "Art Museum",
            Latitude = 45.0,
            Longitude = 19.0
        };
        var keyPoint = await service.AddKeyPointAsync(keyPointRequest, guideId);
        Assert.NotNull(keyPoint);
        Assert.Equal(tour.Id, keyPoint.TourId);
    }

    [Fact]
    public async Task PublishTour_ShouldSetStateToPublished_WhenValid()
    {
        var dbContext = GetInMemoryDbContext();
        var service = new TourService(dbContext);
        var guideId = Guid.NewGuid();
        var tour = await service.CreateTourAsync(new CreateTourRequest
        {
            Name = "Tour",
            Description = "Desc",
            Difficulty = "Hard",
            Category = TourCategory.Sport,
            Price = 200,
            Date = DateTime.UtcNow.AddDays(7)
        }, guideId);
        // Dodaj dva ključna pointa
        await service.AddKeyPointAsync(new AddKeyPointRequest { TourId = tour.Id, Name = "A", Description = "A", Latitude = 1, Longitude = 1 }, guideId);
        await service.AddKeyPointAsync(new AddKeyPointRequest { TourId = tour.Id, Name = "B", Description = "B", Latitude = 2, Longitude = 2 }, guideId);
        var publishedTour = await service.PublishTourAsync(tour.Id, guideId);
        Assert.Equal(TourState.Published, publishedTour.State);
    }

    [Fact]
    public async Task CancelTour_ShouldSetStateToCancelled_AndGiveBonusPoints()
    {
        var dbContext = GetInMemoryDbContext();
        var service = new TourService(dbContext);
        var guideId = Guid.NewGuid();
        var touristId = Guid.NewGuid();
        var tour = await service.CreateTourAsync(new CreateTourRequest
        {
            Name = "Tour",
            Description = "Desc",
            Difficulty = "Medium",
            Category = TourCategory.Food,
            Price = 150,
            Date = DateTime.UtcNow.AddDays(3)
        }, guideId);
        // Dodaj dva ključna pointa
        await service.AddKeyPointAsync(new AddKeyPointRequest { TourId = tour.Id, Name = "A", Description = "A", Latitude = 1, Longitude = 1 }, guideId);
        await service.AddKeyPointAsync(new AddKeyPointRequest { TourId = tour.Id, Name = "B", Description = "B", Latitude = 2, Longitude = 2 }, guideId);
        await service.PublishTourAsync(tour.Id, guideId);
        // Dodaj turistu i kupovinu
        dbContext.Users.Add(new User { Id = touristId, Username = "t", PasswordHash = "x", Role = UserRole.Tourist, Email = "t@t.com" });
        dbContext.Purchases.Add(new Purchase { Id = Guid.NewGuid(), TouristId = touristId, TourId = tour.Id, PurchaseDate = DateTime.UtcNow, UsedBonusPoints = 0, FinalPrice = 150 });
        await dbContext.SaveChangesAsync();
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
        
        var cancelledTour = await service.CancelTourAsync(tour.Id, guideId, emailService);
        Assert.Equal(TourState.Cancelled, cancelledTour.State);
        var tourist = await dbContext.Users.FindAsync(touristId);
        Assert.Equal(150, tourist.BonusPoints);
    }
} 