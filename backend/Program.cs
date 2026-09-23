using Microsoft.EntityFrameworkCore;
using PlacementTracker.Api.Data;

// This app stores plain calendar dates (AppliedOn, often Unspecified-Kind)
// alongside UTC stamps (CreatedAt). The legacy timestamp behavior accepts
// all DateTime Kinds instead of rejecting mixed Kinds.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// API-only: controllers return JSON, no Razor views.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL via configuration / environment variables. No hardcoded secrets.
// Priority: ConnectionStrings__Default env var > appsettings.json > fallback.
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
    ?? "Host=127.0.0.1;Port=5432;Database=placement_tracker;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// CORS: allow the Vue dev server and the NGINX entry point to call the API.
// http://localhost:8080 is required because browsers load the app from the
// NGINX origin; any cross-origin request from that origin (absolute-URL API
// calls, Swagger/tooling, non-simple requests) would otherwise be rejected.
// Same-origin /api requests proxied by NGINX need no CORS headers, but the
// allow-list entry makes :8080 a first-class origin. :5173 is unchanged.
const string VueDevPolicy = "VueDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(VueDevPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:8080")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(VueDevPolicy);
app.UseAuthorization();
app.MapControllers();

// Create tables if needed (never destructive). Skipped in Testing so tests
// use isolated providers and never touch the real PostgreSQL database.
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
                new PlacementTracker.Api.Models.Application
                {
                    Company = "Acme Labs",
                    Role = "Python Intern",
                    Status = "Applied",
                    AppliedOn = new DateTime(2026, 9, 16),
                    JobUrl = "https://example.com/jobs/acme-python-intern",
                    Notes = "Sample record.",
                    CreatedAt = DateTime.UtcNow
                },
                new PlacementTracker.Api.Models.Application
                {
                    Company = "Northstar",
                    Role = "Graduate Engineer",
                    Status = "Interview",
                    AppliedOn = new DateTime(2026, 9, 12),
                    Notes = "Sample record.",
                    CreatedAt = DateTime.UtcNow
                },
                new PlacementTracker.Api.Models.Application
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
