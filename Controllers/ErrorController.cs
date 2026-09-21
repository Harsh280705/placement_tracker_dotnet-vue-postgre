using Microsoft.AspNetCore.Mvc;

namespace PlacementTracker.Controllers;

// Renders the custom 404 page for unknown routes (via
// UseStatusCodePagesWithReExecute("/Error/{0}")).
public class ErrorController : Controller
{
    [Route("/Error/{code:int}")]
    [Route("/Error")]
    public IActionResult Index(int code = 404)
    {
        Response.StatusCode = code;
        if (code == 404)
        {
            return View("~/Views/Shared/404.cshtml");
        }
        return View("~/Views/Shared/404.cshtml");
    }
}
