using System.ComponentModel.DataAnnotations;

namespace PlacementTracker.Models;

// One instance = one job/internship application row in SQLite.
public class Application : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Company is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Company must be 2-100 characters.")]
    public string Company { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Role must be 2-100 characters.")]
    public string Role { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = "Applied";

    public static readonly string[] AllowedStatuses =
        new[] { "Wishlist", "Applied", "Interview", "Offer", "Rejected" };

    [DataType(DataType.Date)]
    [Display(Name = "Applied On")]
    public DateTime? AppliedOn { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL (e.g. https://example.com/job).")]
    [Display(Name = "Job URL")]
    public string? JobUrl { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot be longer than 1000 characters.")]
    public string? Notes { get; set; }

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Business rules that need more than one attribute:
    // - Status must be one of the allowed values.
    // - AppliedOn is required unless Status is Wishlist.
    // - Blank/whitespace company/role is rejected.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!AllowedStatuses.Contains(Status))
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
