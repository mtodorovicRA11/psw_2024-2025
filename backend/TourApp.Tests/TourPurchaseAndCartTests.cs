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

public class TourPurchaseAndCartTests
{
    private TourAppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TourAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TourAppDbContext(options);
    }

    [Fact]
    public async Task PurchaseTour_ShouldDeductBonusPoints_AndCreatePurchase()
    {
        var dbContext = GetInMemoryDbContext();
        var service = new TourService(dbContext);
        var touristId = Guid.NewGuid();
        var guideId = Guid.NewGuid();
        var tour = await service.CreateTourAsync(new CreateTourRequest
        {
            Name = "Tour",
            Description = "Desc",
            Difficulty = "Medium",
            Category = TourCategory.Food,
            Price = 100,
            Date = DateTime.UtcNow.AddDays(5)
        }, guideId);
        // Dodaj dva ključna pointa i objavi turu
        await service.AddKeyPointAsync(new AddKeyPointRequest { TourId = tour.Id, Name = "A", Description = "A", Latitude = 1, Longitude = 1 }, guideId);
        await service.AddKeyPointAsync(new AddKeyPointRequest { TourId = tour.Id, Name = "B", Description = "B", Latitude = 2, Longitude = 2 }, guideId);
        await service.PublishTourAsync(tour.Id, guideId);
        dbContext.Users.Add(new User { Id = touristId, Username = "t", PasswordHash = "x", Role = UserRole.Tourist, Email = "t@t.com", BonusPoints = 50 });
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
        
        var purchase = await service.PurchaseTourAsync(new PurchaseTourRequest { TourId = tour.Id, UseBonusPoints = 30 }, touristId, emailService);
        Assert.NotNull(purchase);
        Assert.Equal(70, purchase.FinalPrice); // 100 - 30
        var tourist = await dbContext.Users.FindAsync(touristId);
        Assert.Equal(20, tourist.BonusPoints); // 50 - 30
    }

    [Fact]
    public async Task AddToCart_And_RemoveFromCart_ShouldWork()
    {
        var dbContext = GetInMemoryDbContext();
        var service = new TourService(dbContext);
        var touristId = Guid.NewGuid();
        var guideId = Guid.NewGuid();
        var tour = await service.CreateTourAsync(new CreateTourRequest
        {
            Name = "Tour",
            Description = "Desc",
            Difficulty = "Medium",
            Category = TourCategory.Food,
            Price = 100,
            Date = DateTime.UtcNow.AddDays(5)
        }, guideId);
        await service.AddToCartAsync(touristId, tour.Id);
        var cart = await service.GetCartAsync(touristId);
        Assert.Single(cart.Items);
        await service.RemoveFromCartAsync(touristId, tour.Id);
        var cartAfter = await service.GetCartAsync(touristId);
        Assert.Empty(cartAfter.Items);
    }

    [Fact]
    public async Task PurchaseTour_ShouldRemoveFromCart()
    {
        var dbContext = GetInMemoryDbContext();
        var service = new TourService(dbContext);
        var touristId = Guid.NewGuid();
        var guideId = Guid.NewGuid();
        
        // Create tour and add to cart
        var tour = await service.CreateTourAsync(new CreateTourRequest
        {
            Name = "Tour",
            Description = "Desc",
            Difficulty = "Medium",
            Category = TourCategory.Food,
            Price = 100,
            Date = DateTime.UtcNow.AddDays(5)
        }, guideId);
        
        // Add key points and publish tour
        await service.AddKeyPointAsync(new AddKeyPointRequest { TourId = tour.Id, Name = "A", Description = "A", Latitude = 1, Longitude = 1 }, guideId);
        await service.AddKeyPointAsync(new AddKeyPointRequest { TourId = tour.Id, Name = "B", Description = "B", Latitude = 2, Longitude = 2 }, guideId);
        await service.PublishTourAsync(tour.Id, guideId);
        
        // Add user and tour to cart
        dbContext.Users.Add(new User { Id = touristId, Username = "t", PasswordHash = "x", Role = UserRole.Tourist, Email = "t@t.com", BonusPoints = 50 });
        await dbContext.SaveChangesAsync();
        
        await service.AddToCartAsync(touristId, tour.Id);
        var cartBefore = await service.GetCartAsync(touristId);
        Assert.Single(cartBefore.Items);
        
        // Purchase the tour
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
        
        var purchase = await service.PurchaseTourAsync(new PurchaseTourRequest { TourId = tour.Id, UseBonusPoints = 0 }, touristId, emailService);
        Assert.NotNull(purchase);
        
        // Verify tour is removed from cart
        var cartAfter = await service.GetCartAsync(touristId);
        Assert.Empty(cartAfter.Items);
    }
} 