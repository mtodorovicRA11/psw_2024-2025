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
    public async Task ReportProblem_ShouldCreateProblem()
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
        
        var problem = await service.ReportProblemAsync(new ReportProblemRequest { TourId = tour.Id, Title = "Problem", Description = "Opis problema" }, touristId, emailService);
        Assert.NotNull(problem);
        Assert.Equal(ProblemStatus.Pending, problem.Status);
    }

    [Fact]
    public async Task UpdateProblemStatus_ShouldChangeStatus_WhenAdmin()
    {
        var dbContext = GetInMemoryDbContext();
        var service = new TourService(dbContext);
        var adminId = Guid.NewGuid();
        var touristId = Guid.NewGuid();
        var guideId = Guid.NewGuid();
        
        // Create users
        dbContext.Users.Add(new User { Id = adminId, Username = "admin", PasswordHash = "x", Role = UserRole.Admin, Email = "admin@test.com" });
        dbContext.Users.Add(new User { Id = touristId, Username = "tourist", PasswordHash = "x", Role = UserRole.Tourist, Email = "tourist@test.com" });
        dbContext.Users.Add(new User { Id = guideId, Username = "guide", PasswordHash = "x", Role = UserRole.Guide, Email = "guide@test.com" });
        
        // Create tour and problem
        var tour = await service.CreateTourAsync(new CreateTourRequest
        {
            Name = "Tour",
            Description = "Desc",
            Difficulty = "Medium",
            Category = TourCategory.Food,
            Price = 100,
            Date = DateTime.UtcNow.AddDays(-2)
        }, guideId);
        
        var problem = new TourProblem
        {
            Id = Guid.NewGuid(),
            TourId = tour.Id,
            TouristId = touristId,
            Title = "Test Problem",
            Description = "Test Description",
            Status = ProblemStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.TourProblems.Add(problem);
        await dbContext.SaveChangesAsync();
        
        // Test updating status as admin
        var updateRequest = new UpdateProblemStatusRequest
        {
            ProblemId = problem.Id,
            NewStatus = ProblemStatus.UnderReview
        };
        
        var updatedProblem = await service.UpdateProblemStatusAsync(updateRequest, adminId, UserRole.Admin);
        
        Assert.NotNull(updatedProblem);
        Assert.Equal(ProblemStatus.UnderReview, updatedProblem.Status);
        
        // Test updating to resolved
        updateRequest.NewStatus = ProblemStatus.Resolved;
        var resolvedProblem = await service.UpdateProblemStatusAsync(updateRequest, adminId, UserRole.Admin);
        
        Assert.Equal(ProblemStatus.Resolved, resolvedProblem.Status);
    }
} 