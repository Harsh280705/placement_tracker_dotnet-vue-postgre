using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlacementTracker.Api.Data;

namespace PlacementTracker.Api.Tests;

// Full HTTP pipeline tests against /api/applications with an isolated
// InMemory database. The app runs with Environment="Testing" so Program.cs
// never touches the real PostgreSQL database.
public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        // NOTE: the db name must be created OUTSIDE ConfigureServices:
        // the delegate can run more than once and Guid.NewGuid() inside it
        // would give different scopes different isolated stores.
        var dbName = "IntegrationTests-" + Guid.NewGuid();
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
                    options.UseInMemoryDatabase(dbName));
            });
        });
    }

    [Fact]
    public async Task Get_all_returns_200_with_json_array()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/applications");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<DTOs.ApplicationResponseDto>>();
        Assert.NotNull(list);
    }

    [Fact]
    public async Task Get_nonexistent_returns_404()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/applications/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Full_crud_roundtrip_over_http()
    {
        var client = _factory.CreateClient();

        // POST valid -> 201
        var post = await client.PostAsJsonAsync("/api/applications", new
        {
            company = "Acme Labs",
            role = "Backend Intern",
            status = "Applied",
            appliedOn = "2026-09-16",
            jobUrl = "https://example.com/job/123",
            notes = "Integration test"
        });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        var created = await post.Content.ReadFromJsonAsync<DTOs.ApplicationResponseDto>();
        Assert.NotNull(created);
        Assert.Equal("Acme Labs", created!.Company);

        // GET one -> 200
        var get = await client.GetAsync($"/api/applications/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        // PUT valid -> 200
        var put = await client.PutAsJsonAsync($"/api/applications/{created.Id}", new
        {
            company = "Contoso",
            role = "Offer Role",
            status = "Offer",
            appliedOn = "2026-09-05",
            jobUrl = "https://example.com/offer",
            notes = "Updated"
        });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        // DELETE -> 204
        var delete = await client.DeleteAsync($"/api/applications/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        // GET after delete -> 404
        var getAfter = await client.GetAsync($"/api/applications/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfter.StatusCode);
    }

    [Fact]
    public async Task Post_invalid_returns_400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/applications", new
        {
            company = "",
            role = "x",
            status = "Hired",
            appliedOn = (string?)null
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_nonexistent_returns_404()
    {
        var client = _factory.CreateClient();
        var response = await client.PutAsJsonAsync("/api/applications/999999", new
        {
            company = "Contoso",
            role = "Backend Intern",
            status = "Applied",
            appliedOn = "2026-09-05"
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_nonexistent_returns_404()
    {
        var client = _factory.CreateClient();
        var response = await client.DeleteAsync("/api/applications/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
