using LolBuilds.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace LolBuilds.Api.Controllers;

// Small shared helpers for all controllers.
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    // The logged-in user's ID, read from the "sub" claim of the JWT.
    // Only use this in [Authorize] actions (there the token is guaranteed to be valid).
    protected int CurrentUserId => int.Parse(User.FindFirst("sub")!.Value);

    protected bool IsAdmin => User.IsInRole(Roles.Admin);

    // Ownership rule: you can change something only if you wrote it.
    // Admins can additionally delete anything (moderation), see the controllers.
    protected bool IsOwner(int authorId) => authorId == CurrentUserId;

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

    // 401: we don't know who you are (no token, bad token, wrong password).
    protected ObjectResult UnauthorizedProblem(string message)
    {
        return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized", detail: message);
    }

    // 403: we know who you are, but you're not allowed to do this.
    protected ObjectResult ForbiddenProblem(string message)
    {
        return Problem(statusCode: StatusCodes.Status403Forbidden, title: "Forbidden", detail: message);
    }
}
