using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementTracker.Data;

namespace PlacementTracker.Controllers;

// Handles every placement-application route from the spec:
//   GET  /
//   GET  /applications/new
//   POST /applications
//   GET  /applications/{id}
//   GET  /applications/{id}/edit
//   POST /applications/{id}/edit
//   POST /applications/{id}/delete
public class ApplicationsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ApplicationsController(ApplicationDbContext db)
    {
        _db = db;
    }

    private void NormalizeOptionalFields(Models.Application app)
    {
        // Empty strings from HTML forms should behave like "not provided".
        // [Url] rejects "" so convert it to null and clear any stale error.
        if (string.IsNullOrWhiteSpace(app.JobUrl))
        {
            app.JobUrl = null;
            ModelState.Remove(nameof(Models.Application.JobUrl));
        }
        if (string.IsNullOrWhiteSpace(app.Notes))
        {
            app.Notes = null;
            ModelState.Remove(nameof(Models.Application.Notes));
        }
        if (string.IsNullOrWhiteSpace(app.Company))
        {
            // Let [Required] + IValidatableObject produce the message.
            app.Company = string.Empty;
        }
        if (string.IsNullOrWhiteSpace(app.Role))
        {
            app.Role = string.Empty;
        }
    }

    private IActionResult CustomNotFound()
    {
        Response.StatusCode = 404;
        return View("~/Views/Shared/404.cshtml");
    }

    // GET /
    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var apps = await _db.Applications
            .OrderByDescending(a => a.Id)
            .ToListAsync();
        return View("Index", apps);
    }

    // GET /applications/new (display create form)
    [HttpGet("/applications/new")]
    public IActionResult New()
    {
        var model = new Models.Application
        {
            Status = "Applied",
            AppliedOn = DateTime.Today
        };
        return View("Create", model);
    }

    // POST /applications (process create form, Post/Redirect/Get)
    [HttpPost("/applications")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Models.Application application)
    {
        NormalizeOptionalFields(application);
        application.CreatedAt = DateTime.UtcNow;

        if (!ModelState.IsValid)
        {
            // Invalid: do NOT save, re-show form with values + messages.
            return View("Create", application);
        }

        _db.Applications.Add(application);          // query: add
        await _db.SaveChangesAsync();               // save
        return RedirectToAction(nameof(Details), new { id = application.Id });
    }

    // GET /applications/{id}
    [HttpGet("/applications/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var app = await _db.Applications.FindAsync(id);
        if (app is null)
        {
            return CustomNotFound();
        }
        return View("Details", app);
    }

    // GET /applications/{id}/edit (pre-filled form)
    [HttpGet("/applications/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var app = await _db.Applications.FindAsync(id);
        if (app is null)
        {
            return CustomNotFound();
        }
        return View("Edit", app);
    }

    // POST /applications/{id}/edit (validate + update + redirect)
    [HttpPost("/applications/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Models.Application application)
    {
        NormalizeOptionalFields(application);

        if (!ModelState.IsValid)
        {
            application.Id = id;
            return View("Edit", application);
        }

        var existing = await _db.Applications.FindAsync(id);
        if (existing is null)
        {
            return CustomNotFound();
        }

        // Update every editable field.
        existing.Company = application.Company.Trim();
        existing.Role = application.Role.Trim();
        existing.Status = application.Status;
        existing.AppliedOn = application.AppliedOn;
        existing.JobUrl = application.JobUrl;
        existing.Notes = application.Notes;

        await _db.SaveChangesAsync();               // update + save
        return RedirectToAction(nameof(Details), new { id = existing.Id });
    }

    // POST /applications/{id}/delete (POST only, never GET)
    [HttpPost("/applications/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var app = await _db.Applications.FindAsync(id);
        if (app is null)
        {
            return CustomNotFound();
        }

        _db.Applications.Remove(app);               // delete
        await _db.SaveChangesAsync();               // save
        return RedirectToAction(nameof(Index));
    }
}
