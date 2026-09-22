using System.ComponentModel.DataAnnotations;
using Moq;

namespace PlacementTracker.Api.Tests;

// Validation rules reused from the original implementation:
// company 2-100, role 2-100, status in allowed set,
// AppliedOn required unless Wishlist, JobUrl valid URL, Notes <= 1000.
public class ValidationTests
{
    private static IList<ValidationResult> Validate(object app)
    {
        // Moq is used here: ValidationContext accepts an IServiceProvider.
        var mockServices = new Mock<IServiceProvider>();
        mockServices.Setup(s => s.GetService(It.IsAny<Type>())).Returns((object?)null);

        var context = new ValidationContext(app, mockServices.Object, null);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(app, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Valid_application_passes_validation()
    {
        Assert.Empty(Validate(TestHelpers.ValidApplication()));
    }

    [Fact]
    public void Valid_create_dto_passes_validation()
    {
        Assert.Empty(Validate(TestHelpers.ValidCreateDto()));
    }

    [Fact]
    public void Wishlist_without_applied_date_is_valid()
    {
        var app = TestHelpers.ValidApplication();
        app.Status = "Wishlist";
        app.AppliedOn = null;
        Assert.Empty(Validate(app));
    }

    [Fact]
    public void Blank_company_fails()
    {
        var app = TestHelpers.ValidApplication();
        app.Company = "";
        Assert.NotEmpty(Validate(app));
    }

    [Fact]
    public void Blank_role_fails()
    {
        var app = TestHelpers.ValidApplication();
        app.Role = "   ";
        Assert.NotEmpty(Validate(app));
    }

    [Fact]
    public void Company_shorter_than_2_chars_fails()
    {
        var app = TestHelpers.ValidApplication();
        app.Company = "A";
        Assert.NotEmpty(Validate(app));
    }

    [Fact]
    public void Role_shorter_than_2_chars_fails()
    {
        var app = TestHelpers.ValidApplication();
        app.Role = "x";
        Assert.NotEmpty(Validate(app));
    }

    [Fact]
    public void Company_longer_than_100_chars_fails()
    {
        var app = TestHelpers.ValidApplication();
        app.Company = new string('a', 101);
        Assert.NotEmpty(Validate(app));
    }

    [Fact]
    public void Role_longer_than_100_chars_fails()
    {
        var app = TestHelpers.ValidApplication();
        app.Role = new string('b', 101);
        Assert.NotEmpty(Validate(app));
    }

    [Fact]
    public void Notes_longer_than_1000_chars_fails()
    {
        var app = TestHelpers.ValidApplication();
        app.Notes = new string('n', 1001);
        Assert.NotEmpty(Validate(app));
    }

    [Fact]
    public void Invalid_status_fails()
    {
        var app = TestHelpers.ValidApplication();
        app.Status = "Hired";
        Assert.NotEmpty(Validate(app));
    }

    [Fact]
    public void Missing_applied_date_when_not_wishlist_fails()
    {
        var app = TestHelpers.ValidApplication();
        app.Status = "Applied";
        app.AppliedOn = null;
        var errors = Validate(app);
        Assert.Contains(errors, e => e.MemberNames.Contains("AppliedOn"));
    }

    [Fact]
    public void Invalid_url_fails()
    {
        var app = TestHelpers.ValidApplication();
        app.JobUrl = "not-a-url";
        Assert.NotEmpty(Validate(app));
    }

    [Fact]
    public void Optional_fields_can_be_empty()
    {
        var app = TestHelpers.ValidApplication();
        app.JobUrl = null;
        app.Notes = null;
        Assert.Empty(Validate(app));
    }
}
