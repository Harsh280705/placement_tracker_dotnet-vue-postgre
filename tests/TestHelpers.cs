using Microsoft.EntityFrameworkCore;
using PlacementTracker.Api.Data;

namespace PlacementTracker.Api.Tests;

// Each test gets its own isolated InMemory database (never the real PostgreSQL).
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

    public static DTOs.CreateApplicationDto ValidCreateDto() => new()
    {
        Company = "Acme Labs",
        Role = "Backend Intern",
        Status = "Applied",
        AppliedOn = new DateTime(2026, 9, 16),
        JobUrl = "https://example.com/job/123",
        Notes = "Test notes"
    };

    public static DTOs.UpdateApplicationDto ValidUpdateDto() => new()
    {
        Company = "Contoso",
        Role = "Offer Role",
        Status = "Offer",
        AppliedOn = new DateTime(2026, 9, 5),
        JobUrl = "https://example.com/offer",
        Notes = "Updated notes"
    };
}
