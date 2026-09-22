using System.ComponentModel.DataAnnotations;

namespace PlacementTracker.Api.DTOs;

// Shared validation for status values (mirrors Application.AllowedStatuses).
public static class ApplicationStatus
{
    public static readonly string[] Allowed = new[] { "Wishlist", "Applied", "Interview", "Offer", "Rejected" };
}

public class CreateApplicationDto : IValidatableObject
{
    [Required(ErrorMessage = "Company is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Company must be 2-100 characters.")]
    public string Company { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Role must be 2-100 characters.")]
    public string Role { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = "Applied";

    public DateTime? AppliedOn { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL (e.g. https://example.com/job).")]
    public string? JobUrl { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot be longer than 1000 characters.")]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!ApplicationStatus.Allowed.Contains(Status))
        {
            yield return new ValidationResult(
                "Status must be one of: Wishlist, Applied, Interview, Offer, Rejected.",
                new[] { nameof(Status) });
        }

        if (string.IsNullOrWhiteSpace(Company) || Company.Trim().Length < 2)
        {
            yield return new ValidationResult(
                "Company must be 2-100 characters.",
                new[] { nameof(Company) });
        }

        if (string.IsNullOrWhiteSpace(Role) || Role.Trim().Length < 2)
        {
            yield return new ValidationResult(
                "Role must be 2-100 characters.",
                new[] { nameof(Role) });
        }

        if (Status != "Wishlist" && AppliedOn is null)
        {
            yield return new ValidationResult(
                "Applied date is required unless status is Wishlist.",
                new[] { nameof(AppliedOn) });
        }
    }
}

public class UpdateApplicationDto : IValidatableObject
{
    [Required(ErrorMessage = "Company is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Company must be 2-100 characters.")]
    public string Company { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Role must be 2-100 characters.")]
    public string Role { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = "Applied";

    public DateTime? AppliedOn { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL (e.g. https://example.com/job).")]
    public string? JobUrl { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot be longer than 1000 characters.")]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!ApplicationStatus.Allowed.Contains(Status))
        {
            yield return new ValidationResult(
                "Status must be one of: Wishlist, Applied, Interview, Offer, Rejected.",
                new[] { nameof(Status) });
        }

        if (string.IsNullOrWhiteSpace(Company) || Company.Trim().Length < 2)
        {
            yield return new ValidationResult(
                "Company must be 2-100 characters.",
                new[] { nameof(Company) });
        }

        if (string.IsNullOrWhiteSpace(Role) || Role.Trim().Length < 2)
        {
            yield return new ValidationResult(
                "Role must be 2-100 characters.",
                new[] { nameof(Role) });
        }

        if (Status != "Wishlist" && AppliedOn is null)
        {
            yield return new ValidationResult(
                "Applied date is required unless status is Wishlist.",
                new[] { nameof(AppliedOn) });
        }
    }
}

public class ApplicationResponseDto
{
    public int Id { get; set; }
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? AppliedOn { get; set; }
    public string? JobUrl { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public static ApplicationResponseDto FromEntity(Models.Application app) => new()
    {
        Id = app.Id,
        Company = app.Company,
        Role = app.Role,
        Status = app.Status,
        AppliedOn = app.AppliedOn,
        JobUrl = app.JobUrl,
        Notes = app.Notes,
        CreatedAt = app.CreatedAt
    };
}
