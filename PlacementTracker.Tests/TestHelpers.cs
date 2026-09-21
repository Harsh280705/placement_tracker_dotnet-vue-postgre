using Microsoft.EntityFrameworkCore;
using PlacementTracker.Data;

namespace PlacementTracker.Tests;

// Each test gets its own isolated InMemory database (never the real SQLite file).
public static class TestHelpers
{
    public static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    public static Models.Application ValidApplication() => new()
    {
        Company = "Acme Labs",
        Role = "Backend Intern",
        Status = "Applied",
        AppliedOn = new DateTime(2026, 9, 16),
        JobUrl = "https://example.com/job/123",
        Notes = "Test notes",
        CreatedAt = DateTime.UtcNow
    };
}
