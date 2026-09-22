using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementTracker.Api.Data;
using PlacementTracker.Api.DTOs;

namespace PlacementTracker.Api.Controllers;

// JSON-only API. Replaces the old MVC view-rendering ApplicationsController.
[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ApplicationsController(ApplicationDbContext db)
    {
        _db = db;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    // GET /api/applications -> 200 OK
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> GetAll()
    {
        var apps = await _db.Applications
            .OrderByDescending(a => a.Id)
            .ToListAsync();
        return Ok(apps.Select(ApplicationResponseDto.FromEntity).ToList());
    }

    // GET /api/applications/{id} -> 200 OK or 404 Not Found
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApplicationResponseDto>> GetOne(int id)
    {
        var app = await _db.Applications.FindAsync(id);
        if (app is null)
        {
            return NotFound(new { message = $"Application {id} not found." });
        }
        return Ok(ApplicationResponseDto.FromEntity(app));
    }

    // POST /api/applications -> 201 Created or 400 Bad Request
    [HttpPost]
    public async Task<ActionResult<ApplicationResponseDto>> Create([FromBody] CreateApplicationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var app = new Models.Application
        {
            Company = dto.Company.Trim(),
            Role = dto.Role.Trim(),
            Status = dto.Status,
            AppliedOn = dto.AppliedOn,
            JobUrl = NormalizeOptional(dto.JobUrl),
            Notes = NormalizeOptional(dto.Notes),
            CreatedAt = DateTime.UtcNow
        };

        _db.Applications.Add(app);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetOne),
            new { id = app.Id },
            ApplicationResponseDto.FromEntity(app));
    }

    // PUT /api/applications/{id} -> 200 OK or 400 / 404
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApplicationResponseDto>> Update(int id, [FromBody] UpdateApplicationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var existing = await _db.Applications.FindAsync(id);
        if (existing is null)
        {
            return NotFound(new { message = $"Application {id} not found." });
        }

        existing.Company = dto.Company.Trim();
        existing.Role = dto.Role.Trim();
        existing.Status = dto.Status;
        existing.AppliedOn = dto.AppliedOn;
        existing.JobUrl = NormalizeOptional(dto.JobUrl);
        existing.Notes = NormalizeOptional(dto.Notes);

        await _db.SaveChangesAsync();

        return Ok(ApplicationResponseDto.FromEntity(existing));
    }

    // DELETE /api/applications/{id} -> 204 No Content or 404 Not Found
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var app = await _db.Applications.FindAsync(id);
        if (app is null)
        {
            return NotFound(new { message = $"Application {id} not found." });
        }

        _db.Applications.Remove(app);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
