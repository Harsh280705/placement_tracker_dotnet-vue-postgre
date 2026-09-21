using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlacementTracker.Controllers;

namespace PlacementTracker.Tests;

// Controller + EF (InMemory) tests. Never touches Data/placement_tracker.db.
public class ApplicationsControllerTests
{
    private static ApplicationsController ControllerWithDb(
        PlacementTracker.Data.ApplicationDbContext db)
    {
        var controller = new ApplicationsController(db);
        // CustomNotFound() sets Response.StatusCode, so give the controller an HttpContext.
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact]
    public async Task Dashboard_returns_all_applications()
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
        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Models.Application>>(view.Model);
        Assert.Equal(2, model.Count);
    }

    [Fact]
    public async Task Empty_database_returns_empty_dashboard_model()
    {
        using var db = TestHelpers.CreateDb();
        var controller = ControllerWithDb(db);
        var result = await controller.Index();
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Models.Application>>(view.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task Valid_creation_saves_and_redirects_to_details()
    {
        using var db = TestHelpers.CreateDb();
        var controller = ControllerWithDb(db);
        var app = TestHelpers.ValidApplication();

        var result = await controller.Create(app);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal(1, db.Applications.Count());
        Assert.Equal("Acme Labs", db.Applications.Single().Company);
    }

    [Fact]
    public async Task Invalid_creation_does_not_save_and_returns_form()
    {
        using var db = TestHelpers.CreateDb();
        var controller = ControllerWithDb(db);
        controller.ModelState.AddModelError("Company", "Company is required.");
        var app = TestHelpers.ValidApplication();
        app.Company = "";

        var result = await controller.Create(app);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Create", view.ViewName);
        Assert.Equal(0, db.Applications.Count());
    }

    [Fact]
    public async Task Details_shows_existing_application()
    {
        using var db = TestHelpers.CreateDb();
        db.Applications.Add(TestHelpers.ValidApplication());
        await db.SaveChangesAsync();
        var id = db.Applications.Single().Id;

        var controller = ControllerWithDb(db);
        var result = await controller.Details(id);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Models.Application>(view.Model);
        Assert.Equal("Acme Labs", model.Company);
        Assert.Equal("Backend Intern", model.Role);
    }

    [Fact]
    public async Task Unknown_id_returns_404()
    {
        using var db = TestHelpers.CreateDb();
        var controller = ControllerWithDb(db);

        var result = await controller.Details(999999);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Shared/404.cshtml", view.ViewName);
        Assert.Equal(404, controller.Response.StatusCode);
    }

    [Fact]
    public async Task Edit_page_returns_prefilled_model()
    {
        using var db = TestHelpers.CreateDb();
        db.Applications.Add(TestHelpers.ValidApplication());
        await db.SaveChangesAsync();
        var id = db.Applications.Single().Id;

        var controller = ControllerWithDb(db);
        var result = await controller.Edit(id);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Models.Application>(view.Model);
        Assert.Equal(id, model.Id);
        Assert.Equal("Acme Labs", model.Company);
    }

    [Fact]
    public async Task Update_persists_every_field_and_redirects()
    {
        using var db = TestHelpers.CreateDb();
        db.Applications.Add(TestHelpers.ValidApplication());
        await db.SaveChangesAsync();
        var id = db.Applications.Single().Id;

        var controller = ControllerWithDb(db);
        var updated = new Models.Application
        {
            Company = "Contoso",
            Role = "Offer Role",
            Status = "Offer",
            AppliedOn = new DateTime(2026, 9, 5),
            JobUrl = "https://example.com/offer",
            Notes = "Updated notes"
        };

        var result = await controller.Edit(id, updated);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        var saved = db.Applications.Single();
        Assert.Equal("Contoso", saved.Company);
        Assert.Equal("Offer Role", saved.Role);
        Assert.Equal("Offer", saved.Status);
        Assert.Equal("https://example.com/offer", saved.JobUrl);
        Assert.Equal("Updated notes", saved.Notes);
    }

    [Fact]
    public async Task Delete_removes_record_and_redirects_to_dashboard()
    {
        using var db = TestHelpers.CreateDb();
        db.Applications.Add(TestHelpers.ValidApplication());
        await db.SaveChangesAsync();
        var id = db.Applications.Single().Id;

        var controller = ControllerWithDb(db);
        var result = await controller.Delete(id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal(0, db.Applications.Count());
    }
}
