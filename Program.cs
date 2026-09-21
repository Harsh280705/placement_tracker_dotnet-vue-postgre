using Microsoft.EntityFrameworkCore;
using PlacementTracker.Data;

var builder = WebApplication.CreateBuilder(args);

// MVC + Razor Views
builder.Services.AddControllersWithViews();

// SQLite (persistent file, NOT in-memory).
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=Data/placement_tracker.db";

// Make sure the Data/ folder exists so SQLite can create the file.
var dbFile = connectionString.Replace("Data Source=", "").Trim();
var dbDir = Path.GetDirectoryName(dbFile);
if (!string.IsNullOrEmpty(dbDir))
{
    Directory.CreateDirectory(dbDir);
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

// Error handling: friendly page in production, details in development.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error/500");
}

// Custom pages for 404 etc. (re-executes /Error/{code}).
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Conventional fallback route (attribute routes below take priority).
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Applications}/{action=Index}/{id?}");

// Create the SQLite file/tables if needed (no destructive recreate).
// Skipped entirely in the "Testing" environment so tests never touch the real DB.
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();

        // Seed 3 sample records ONLY in Development when the table is empty.
        if (app.Environment.IsDevelopment() && !db.Applications.Any())
    {
        db.Applications.AddRange(
            new PlacementTracker.Models.Application
            {
                Company = "Acme Labs",
                Role = "Python Intern",
                Status = "Applied",
                AppliedOn = new DateTime(2026, 9, 16),
                JobUrl = "https://example.com/jobs/acme-python-intern",
                Notes = "Sample record.",
                CreatedAt = DateTime.UtcNow
            },
            new PlacementTracker.Models.Application
            {
                Company = "Northstar",
                Role = "Graduate Engineer",
                Status = "Interview",
                AppliedOn = new DateTime(2026, 9, 12),
                Notes = "Sample record.",
                CreatedAt = DateTime.UtcNow
            },
            new PlacementTracker.Models.Application
            {
                Company = "Contoso",
                Role = "Backend Intern",
                Status = "Rejected",
                AppliedOn = new DateTime(2026, 9, 5),
                Notes = "Sample record.",
                CreatedAt = DateTime.UtcNow
            });
        db.SaveChanges();
        }
    }
}

app.Run();

// Needed so WebApplicationFactory (integration tests) can find Program.
public partial class Program { }
