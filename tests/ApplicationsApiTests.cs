using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlacementTracker.Api.Controllers;
using PlacementTracker.Api.DTOs;

namespace PlacementTracker.Api.Tests;

// API controller tests with isolated InMemory EF provider. Never touches PostgreSQL.
public class ApplicationsApiTests
{
    private static ApplicationsController ControllerWithDb(Data.ApplicationDbContext db)
    {
        var controller = new ApplicationsController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact]
    public async Task GetAll_returns_all_applications_with_200()
    {
        using var db = TestHelpers.CreateDb();
        db.Applications.Add(TestHelpers.ValidApplication());
        db.Applications.Add(new Models.Application
        {
            Company = "Northstar",
            Role = "Graduate Engineer",
            Status = "Interview",
            AppliedOn = new DateTime(2026, 9, 12),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var controller = ControllerWithDb(db);
        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<List<ApplicationResponseDto>>(ok.Value);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task GetAll_empty_database_returns_empty_list()
    {
        using var db = TestHelpers.CreateDb();
        var controller = ControllerWithDb(db);
        var result = await controller.GetAll();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<List<ApplicationResponseDto>>(ok.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetOne_existing_returns_200_with_dto()
    {
        using var db = TestHelpers.CreateDb();
        db.Applications.Add(TestHelpers.ValidApplication());
        await db.SaveChangesAsync();
        var id = db.Applications.Single().Id;

        var controller = ControllerWithDb(db);
        var result = await controller.GetOne(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ApplicationResponseDto>(ok.Value);
        Assert.Equal("Acme Labs", dto.Company);
        Assert.Equal("Backend Intern", dto.Role);
    }

    [Fact]
    public async Task GetOne_nonexistent_returns_404()
    {
        using var db = TestHelpers.CreateDb();
        var controller = ControllerWithDb(db);
        var result = await controller.GetOne(999999);
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Post_valid_returns_201_and_saves()
    {
        using var db = TestHelpers.CreateDb();
        var controller = ControllerWithDb(db);

        var result = await controller.Create(TestHelpers.ValidCreateDto());

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(1, db.Applications.Count());
        Assert.Equal("Acme Labs", db.Applications.Single().Company);
    }

    [Fact]
    public async Task Post_invalid_returns_400_and_does_not_save()
    {
        using var db = TestHelpers.CreateDb();
        var controller = ControllerWithDb(db);
        controller.ModelState.AddModelError("Company", "Company is required.");
        var dto = TestHelpers.ValidCreateDto();
        dto.Company = "";

        var result = await controller.Create(dto);

        // ValidationProblem() returns an ObjectResult carrying ValidationProblemDetails.
        // The exact 400 wire status is verified by the HTTP integration tests.
        var badRequest = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.Equal(0, db.Applications.Count());
    }

    [Fact]
    public async Task Put_valid_updates_every_field_and_returns_200()
    {
        using var db = TestHelpers.CreateDb();
        db.Applications.Add(TestHelpers.ValidApplication());
        await db.SaveChangesAsync();
        var id = db.Applications.Single().Id;

        var controller = ControllerWithDb(db);
        var result = await controller.Update(id, TestHelpers.ValidUpdateDto());

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ApplicationResponseDto>(ok.Value);
        Assert.Equal("Contoso", dto.Company);
        var saved = db.Applications.Single();
        Assert.Equal("Contoso", saved.Company);
        Assert.Equal("Offer Role", saved.Role);
        Assert.Equal("Offer", saved.Status);
        Assert.Equal("https://example.com/offer", saved.JobUrl);
        Assert.Equal("Updated notes", saved.Notes);
    }

    [Fact]
    public async Task Put_nonexistent_returns_404()
    {
        using var db = TestHelpers.CreateDb();
        var controller = ControllerWithDb(db);
        var result = await controller.Update(999999, TestHelpers.ValidUpdateDto());
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Put_invalid_returns_400()
    {
        using var db = TestHelpers.CreateDb();
        db.Applications.Add(TestHelpers.ValidApplication());
        await db.SaveChangesAsync();
        var id = db.Applications.Single().Id;

        var controller = ControllerWithDb(db);
        controller.ModelState.AddModelError("Status", "Invalid status.");
        var dto = TestHelpers.ValidUpdateDto();
        dto.Status = "Hired";

        var result = await controller.Update(id, dto);
        var badRequest = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.IsType<ValidationProblemDetails>(badRequest.Value);
    }

    [Fact]
    public async Task Delete_existing_returns_204_and_removes_record()
    {
        using var db = TestHelpers.CreateDb();
        db.Applications.Add(TestHelpers.ValidApplication());
        await db.SaveChangesAsync();
        var id = db.Applications.Single().Id;

        var controller = ControllerWithDb(db);
        var result = await controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, db.Applications.Count());
    }

    [Fact]
    public async Task Delete_nonexistent_returns_404()
    {
        using var db = TestHelpers.CreateDb();
        var controller = ControllerWithDb(db);
        var result = await controller.Delete(999999);
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
