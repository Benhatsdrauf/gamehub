using GameHub.Application.Common.Errors;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    // The single, shared place that turns an application-layer Error into an
    // HTTP failure response. HTTP status codes live here in the API layer only;
    // the Application layer never knows what a "409" is.
    protected IActionResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Message
        };

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
