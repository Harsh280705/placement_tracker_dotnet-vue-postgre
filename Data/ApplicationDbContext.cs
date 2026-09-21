using Microsoft.EntityFrameworkCore;
using PlacementTracker.Models;

namespace PlacementTracker.Data;

// EF Core bridge: Controller -> DbContext -> SQLite.
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Models.Application> Applications => Set<Models.Application>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Models.Application>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Company).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Role).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Status).IsRequired().HasMaxLength(20);
            entity.Property(a => a.Notes).HasMaxLength(1000);
        });
    }
}
