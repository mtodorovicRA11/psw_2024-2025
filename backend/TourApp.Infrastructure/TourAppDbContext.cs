using Microsoft.EntityFrameworkCore;
using TourApp.Domain;

namespace TourApp.Infrastructure;

public class TourAppDbContext : DbContext
{
    public TourAppDbContext(DbContextOptions<TourAppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<KeyPoint> KeyPoints => Set<KeyPoint>();
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<TourRating> TourRatings => Set<TourRating>();
    public DbSet<TourProblem> TourProblems => Set<TourProblem>();
    public DbSet<TourProblemEvent> TourProblemEvents => Set<TourProblemEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tourapp");
        // Enum to string conversions
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();
        modelBuilder.Entity<Tour>()
            .Property(t => t.Category)
            .HasConversion<string>();
        modelBuilder.Entity<Tour>()
            .Property(t => t.State)
            .HasConversion<string>();
        modelBuilder.Entity<TourProblem>()
            .Property(p => p.Status)
            .HasConversion<string>();
        // Interests as comma separated string
        modelBuilder.Entity<User>()
            .Property(u => u.Interests)
            .HasConversion(
                v => string.Join(",", v),
                v => v.Split(',', System.StringSplitOptions.RemoveEmptyEntries).Select(x => Enum.Parse<Interest>(x)).ToList()
            );
    }
} 