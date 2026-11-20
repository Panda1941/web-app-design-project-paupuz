using LolBuilds.Api.Data;
using LolBuilds.Api.Dtos;
using LolBuilds.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LolBuilds.Api.Controllers;

// Comment routes span the whole hierarchy: champion -> build -> comment.
[Route("api/v1/champions/{championId:int}/builds/{buildId:int}/comments")]
public class CommentsController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public CommentsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Get a paged list of a build's comments.</summary>
    /// <param name="championId">Champion ID.</param>
    /// <param name="buildId">Build ID (must belong to the champion).</param>
    /// <param name="search">Optional filter: text the comment contains (case-insensitive).</param>
    /// <param name="page">Page number, starting at 1.</param>
    /// <param name="pageSize">Items per page (1-50).</param>
    /// <response code="200">Success.</response>
    /// <response code="404">Not found, or it does not belong to the parent in the URL.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<CommentDto>>> GetAll(
        int championId, int buildId, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (!await BuildExists(championId, buildId))
            return NotFoundProblem($"Build {buildId} was not found for champion {championId}.");

        (page, pageSize) = NormalizePaging(page, pageSize);

        IQueryable<Comment> query = _db.Comments.Where(c => c.BuildId == buildId);
        string extraQuery = "";
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => EF.Functions.Like(c.Text, $"%{search}%"));
            extraQuery = $"&search={Uri.EscapeDataString(search)}";
        }

        int totalCount = await query.CountAsync();
        List<Comment> comments = await query
            .Include(c => c.Author)
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<CommentDto>();
        foreach (Comment comment in comments)
            items.Add(Mapping.ToDto(comment, championId));

        string basePath = $"/api/v1/champions/{championId}/builds/{buildId}/comments";
        int totalPages = TotalPages(totalCount, pageSize);
        var links = Mapping.PageLinks(basePath, extraQuery, page, pageSize, totalPages);
        links.Add(new LinkDto($"/api/v1/champions/{championId}/builds/{buildId}", "build", "GET"));
        return Ok(new PagedResponse<CommentDto>(items, page, pageSize, totalCount, totalPages, links));
    }

    /// <summary>Get one comment of a build.</summary>
    /// <response code="200">Success.</response>
    /// <response code="404">Not found, or it does not belong to the parent in the URL.</response>
    [HttpGet("{commentId:int}")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> GetOne(int championId, int buildId, int commentId)
    {
        Comment? comment = await FindComment(championId, buildId, commentId);
        if (comment == null)
            return NotFoundProblem($"Comment {commentId} was not found for build {buildId} of champion {championId}.");

        return Ok(Mapping.ToDto(comment, championId));
    }

    /// <summary>Create a comment under a build. Logged-in users only.</summary>
    /// <response code="401">Not logged in, or the token is missing, invalid or expired.</response>
    /// <response code="201">Created. The Location header points to the new resource.</response>
    /// <response code="400">The body is not valid JSON or has wrong value types.</response>
    /// <response code="404">Not found, or it does not belong to the parent in the URL.</response>
    /// <response code="422">The JSON is valid, but the values break validation rules.</response>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CommentDto>> Create(int championId, int buildId, CommentRequest request)
    {
        if (!await BuildExists(championId, buildId))
            return NotFoundProblem($"Build {buildId} was not found for champion {championId}.");

        var comment = new Comment
        {
            BuildId = buildId,
            Text = request.Text.Trim(),
            AuthorId = CurrentUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        Comment created = (await FindComment(championId, buildId, comment.Id))!;
        return CreatedAtAction(nameof(GetOne), new { championId, buildId, commentId = comment.Id },
            Mapping.ToDto(created, championId));
    }

    /// <summary>Update a comment. Only its author can do this.</summary>
    /// <response code="401">Not logged in, or the token is missing, invalid or expired.</response>
    /// <response code="403">Logged in, but not allowed (wrong role, or not the author).</response>
    /// <response code="200">Success.</response>
    /// <response code="400">The body is not valid JSON or has wrong value types.</response>
    /// <response code="404">Not found, or it does not belong to the parent in the URL.</response>
    /// <response code="422">The JSON is valid, but the values break validation rules.</response>
    [Authorize]
    [HttpPut("{commentId:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CommentDto>> Update(int championId, int buildId, int commentId, CommentRequest request)
    {
        Comment? comment = await FindComment(championId, buildId, commentId);
        if (comment == null)
            return NotFoundProblem($"Comment {commentId} was not found for build {buildId} of champion {championId}.");
        if (!IsOwner(comment.AuthorId))
            return ForbiddenProblem("You can only edit your own comments.");

        comment.Text = request.Text.Trim();
        comment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(Mapping.ToDto(comment, championId));
    }

    /// <summary>Delete a comment. Its author or an admin can do this.</summary>
    /// <response code="401">Not logged in, or the token is missing, invalid or expired.</response>
    /// <response code="403">Logged in, but not allowed (wrong role, or not the author).</response>
    /// <response code="204">Done, no response body.</response>
    /// <response code="404">Not found, or it does not belong to the parent in the URL.</response>
    [Authorize]
    [HttpDelete("{commentId:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int championId, int buildId, int commentId)
    {
        Comment? comment = await FindComment(championId, buildId, commentId);
        if (comment == null)
            return NotFoundProblem($"Comment {commentId} was not found for build {buildId} of champion {championId}.");
        if (!IsOwner(comment.AuthorId) && !IsAdmin)
            return ForbiddenProblem("You can only delete your own comments.");

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> BuildExists(int championId, int buildId)
    {
        return await _db.Builds.AnyAsync(b => b.Id == buildId && b.ChampionId == championId);
    }

    // Finds a comment only if its build belongs to the champion and the comment belongs to the build.
    private async Task<Comment?> FindComment(int championId, int buildId, int commentId)
    {
        return await _db.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == commentId
                                      && c.BuildId == buildId
                                      && c.Build!.ChampionId == championId);
    }
}
