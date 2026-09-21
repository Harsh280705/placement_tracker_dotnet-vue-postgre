using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlacementTracker.Data;

namespace PlacementTracker.Tests;

// Full HTTP pipeline tests with an isolated InMemory database.
// The app runs with Environment="Testing" so Program.cs never touches
// the real Data/placement_tracker.db file.
public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("IntegrationTests-" + Guid.NewGuid()));
            });
        });
    }

    [Fact]
    public async Task Dashboard_loads_with_empty_state()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("No applications yet", html);
    }

    [Fact]
    public async Task Create_form_loads()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/applications/new");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Add Application", html);
        Assert.Contains("Company", html);
    }

    [Fact]
    public async Task Unknown_id_returns_custom_404()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/applications/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("404", html);
    }
}
