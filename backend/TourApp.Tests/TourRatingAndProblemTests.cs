using Xunit;
using TourApp.Application.DTOs;
using TourApp.Domain;
using TourApp.Application.Services;
using TourApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TourApp.Tests;

public class TourRatingAndProblemTests
{
    private TourAppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TourAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TourAppDbContext(options);
    }

    [Fact]
    public async Task RateTour_ShouldCreateRating_WhenValid()
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
            Price = 100,
            Date = DateTime.UtcNow.AddDays(-2) // prošla tura
        }, guideId);
        // Dodaj dva ključna pointa i objavi turu
        await service.AddKeyPointAsync(new AddKeyPointRequest { TourId = tour.Id, Name = "A", Description = "A", Latitude = 1, Longitude = 1 }, guideId);
        await service.AddKeyPointAsync(new AddKeyPointRequest { TourId = tour.Id, Name = "B", Description = "B", Latitude = 2, Longitude = 2 }, guideId);
        await service.PublishTourAsync(tour.Id, guideId);
        dbContext.Users.Add(new User { Id = touristId, Username = "t", PasswordHash = "x", Role = UserRole.Tourist, Email = "t@t.com" });
        dbContext.Purchases.Add(new Purchase { Id = Guid.NewGuid(), TouristId = touristId, TourId = tour.Id, PurchaseDate = DateTime.UtcNow.AddDays(-3), UsedBonusPoints = 0, FinalPrice = 100 });
        await dbContext.SaveChangesAsync();
        var rating = await service.RateTourAsync(new RateTourRequest { TourId = tour.Id, Rating = 5, Comment = "Odlično!" }, touristId);
        Assert.NotNull(rating);
        Assert.Equal(5, rating.Rating);
        Assert.Equal("Odlično!", rating.Comment);
    }

    [Fact]
    public async Task ReportProblem_ShouldCreateProblemWithEvent()
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
            Price = 100,
            Date = DateTime.UtcNow.AddDays(-2)
        }, guideId);
        await service.AddKeyPointAsync(new AddKeyPointRequest { TourId = tour.Id, Name = "A", Description = "A", Latitude = 1, Longitude = 1 }, guideId);
        await service.AddKeyPointAsync(new AddKeyPointRequest { TourId = tour.Id, Name = "B", Description = "B", Latitude = 2, Longitude = 2 }, guideId);
        await service.PublishTourAsync(tour.Id, guideId);
        dbContext.Users.Add(new User { Id = touristId, Username = "t", PasswordHash = "x", Role = UserRole.Tourist, Email = "t@t.com" });
        dbContext.Purchases.Add(new Purchase { Id = Guid.NewGuid(), TouristId = touristId, TourId = tour.Id, PurchaseDate = DateTime.UtcNow.AddDays(-3), UsedBonusPoints = 0, FinalPrice = 100 });
        await dbContext.SaveChangesAsync();
        var problem = await service.ReportProblemAsync(new ReportProblemRequest { TourId = tour.Id, Title = "Problem", Description = "Opis problema" }, touristId);
        Assert.NotNull(problem);
        Assert.Equal(ProblemStatus.Pending, problem.Status);
        Assert.Single(problem.Events);
        Assert.Equal("Created", problem.Events[0].EventType);
    }
} 