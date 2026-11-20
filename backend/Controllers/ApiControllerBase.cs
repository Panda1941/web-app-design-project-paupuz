using Microsoft.AspNetCore.Mvc;

namespace LolBuilds.Api.Controllers;

// Small shared helpers for all controllers.
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    // TEMPORARY (lab 1): every write is done as the seeded user "faker_fan" (id 2).
    // Lab 2 replaces this with the user ID read from the JWT.
    protected int CurrentUserId => 2;

    // Keeps page numbers sane instead of returning errors for odd values.
    protected static (int page, int pageSize) NormalizePaging(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (page > 100000) page = 100000; // avoids integer overflow when calculating the offset
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 50) pageSize = 50;
        return (page, pageSize);
    }

    protected static int TotalPages(int totalCount, int pageSize)
    {
        return (totalCount + pageSize - 1) / pageSize;
    }

    // 422: the JSON was valid, but its values break a business rule.
    protected ObjectResult Unprocessable(string field, string message)
    {
        var errors = new Dictionary<string, string[]> { [field] = new[] { message } };
        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "One or more validation errors occurred.",
        };
        return UnprocessableEntity(problem);
    }

    protected ObjectResult NotFoundProblem(string message)
    {
        return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not Found", detail: message);
    }

    protected ObjectResult ConflictProblem(string message)
    {
        return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflict", detail: message);
    }
}
