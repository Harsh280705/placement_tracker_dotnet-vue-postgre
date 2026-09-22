using Microsoft.EntityFrameworkCore;
using PlacementTracker.Api.Models;

namespace PlacementTracker.Api.Data;

// EF Core bridge: API Controller -> DbContext -> PostgreSQL.
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
            // Dates carry no timezone in this app (plain calendar dates + UTC stamps),
            // so store them without timezone. This also accepts Unspecified-Kind
            // DateTimes, which Npgsql rejects for "timestamp with time zone".
            entity.Property(a => a.AppliedOn).HasColumnType("timestamp without time zone");
            entity.Property(a => a.CreatedAt).HasColumnType("timestamp without time zone");
        });
    }
}
