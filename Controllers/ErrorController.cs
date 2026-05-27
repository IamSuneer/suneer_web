using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace suneer_web.Controllers;

[Route("error")]
public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    // Handles both:
    //   UseExceptionHandler("/error/500")   → unhandled exceptions
    //   UseStatusCodePagesWithReExecute      → 404, 401, 403 …
    [HttpGet("{statusCode:int}")]
    public IActionResult Handle(int statusCode)
    {
        Response.StatusCode = statusCode;

        // Log unhandled exceptions with full detail
        if (statusCode == 500)
        {
            var ex = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (ex is not null)
            {
                _logger.LogError(ex.Error,
                    "Unhandled exception on path {Path}", ex.Path);
            }
        }

        return statusCode switch
        {
            404 => View("NotFound"),
            _   => View("ServerError")
        };
    }
}
