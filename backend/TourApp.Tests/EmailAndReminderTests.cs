using Xunit;
using TourApp.Application.Services;
using TourApp.Domain;
using TourApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TourApp.Tests;

public class EmailAndReminderTests
{
    [Fact]
    public async Task SendEmailAsync_ShouldNotThrow()
    {
        var emailService = new EmailService();
        await emailService.SendEmailAsync("test@example.com", "Naslov", "Poruka");
        // Ako ne baci izuzetak, stub radi
        Assert.True(true);
    }

    [Fact]
    public async Task SendTourRecommendationsAsync_ShouldNotThrow()
    {
        var options = new DbContextOptionsBuilder<TourAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TourAppDbContext(options);
        var service = new TourService(dbContext);
        var emailService = new EmailService();
        dbContext.Users.Add(new User { Id = Guid.NewGuid(), Username = "t", PasswordHash = "x", Role = UserRole.Tourist, Email = "t@t.com", Interests = new List<Interest> { Interest.Nature } });
        await dbContext.SaveChangesAsync();
        var tour = new Tour { Id = Guid.NewGuid(), Name = "Nature Tour", Description = "Desc", Category = TourCategory.Nature, Price = 100, Date = DateTime.UtcNow.AddDays(5) };
        await service.SendTourRecommendationsAsync(tour, emailService);
        Assert.True(true);
    }
} 