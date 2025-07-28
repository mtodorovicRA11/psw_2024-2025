using Xunit;
using TourApp.Application.Services;
using TourApp.Domain;
using TourApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TourApp.Tests;

public class EmailAndReminderTests
{
    [Fact]
    public async Task SendEmailAsync_ShouldNotThrow()
    {
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
        await emailService.SendEmailAsync("test@example.com", "Naslov", "Poruka");
        // Ako ne baci izuzetak, stub radi
        Assert.True(true);
    }

    [Fact]
    public async Task SendTourRecommendationsAsync_ShouldNotThrow()
    {
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
            
        var options = new DbContextOptionsBuilder<TourAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TourAppDbContext(options);
        var service = new TourService(dbContext);
        var emailService = new EmailService(configuration);
        dbContext.Users.Add(new User { Id = Guid.NewGuid(), Username = "t", PasswordHash = "x", Role = UserRole.Tourist, Email = "t@t.com", Interests = new List<Interest> { Interest.Nature } });
        await dbContext.SaveChangesAsync();
        var tour = new Tour { Id = Guid.NewGuid(), Name = "Nature Tour", Description = "Desc", Category = TourCategory.Nature, Price = 100, Date = DateTime.UtcNow.AddDays(5) };
        await service.SendTourRecommendationsAsync(tour, emailService);
        Assert.True(true);
    }

    [Fact]
    public async Task ScheduledJobService_SendTourRemindersJob_ShouldNotThrow()
    {
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
            
        var options = new DbContextOptionsBuilder<TourAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TourAppDbContext(options);
        var emailService = new EmailService(configuration);
        var scheduledJobService = new ScheduledJobService(dbContext, emailService);
        
        await scheduledJobService.SendTourRemindersJob();
        Assert.True(true);
    }

    [Fact]
    public async Task ScheduledJobService_AwardBestGuidesJob_ShouldNotThrow()
    {
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
            
        var options = new DbContextOptionsBuilder<TourAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TourAppDbContext(options);
        var emailService = new EmailService(configuration);
        var scheduledJobService = new ScheduledJobService(dbContext, emailService);
        
        await scheduledJobService.AwardBestGuidesJob();
        Assert.True(true);
    }

    [Fact]
    public async Task ScheduledJobService_SendTourRecommendationsJob_ShouldNotThrow()
    {
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
            
        var options = new DbContextOptionsBuilder<TourAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TourAppDbContext(options);
        var emailService = new EmailService(configuration);
        var scheduledJobService = new ScheduledJobService(dbContext, emailService);
        
        await scheduledJobService.SendTourRecommendationsJob();
        Assert.True(true);
    }
} 