using LolBuilds.Api.Data;
using LolBuilds.Api.Dtos;
using LolBuilds.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LolBuilds.Api.Controllers;

[Route("api/v1/champions")]
public class ChampionsController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public ChampionsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Get a paged list of champions.</summary>
    /// <param name="name">Optional filter: part of the champion name (case-insensitive).</param>
    /// <param name="page">Page number, starting at 1.</param>
    /// <param name="pageSize">Items per page (1-50).</param>
    /// <response code="200">Success.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ChampionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ChampionDto>>> GetAll(
        [FromQuery] string? name, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        (page, pageSize) = NormalizePaging(page, pageSize);

        IQueryable<Champion> query = _db.Champions;
        string extraQuery = "";
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{name}%"));
            extraQuery = $"&name={Uri.EscapeDataString(name)}";
        }

        int totalCount = await query.CountAsync();
        List<Champion> champions = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<ChampionDto>();
        foreach (Champion champion in champions)
            items.Add(Mapping.ToDto(champion));

        int totalPages = TotalPages(totalCount, pageSize);
        var links = Mapping.PageLinks("/api/v1/champions", extraQuery, page, pageSize, totalPages);
        return Ok(new PagedResponse<ChampionDto>(items, page, pageSize, totalCount, totalPages, links));
    }

    /// <summary>Get one champion.</summary>
    /// <response code="200">Success.</response>
    /// <response code="404">Not found, or it does not belong to the parent in the URL.</response>
    [HttpGet("{championId:int}")]
    [ProducesResponseType(typeof(ChampionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChampionDto>> GetOne(int championId)
    {
        Champion? champion = await _db.Champions.FindAsync(championId);
        if (champion == null)
            return NotFoundProblem($"Champion {championId} was not found.");

        return Ok(Mapping.ToDto(champion));
    }

    /// <summary>Get a champion overview: champion info, build and comment counts, and the latest builds.</summary>
    /// <response code="200">Success.</response>
    /// <response code="404">Not found, or it does not belong to the parent in the URL.</response>
    [HttpGet("{championId:int}/summary")]
    [ProducesResponseType(typeof(ChampionSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChampionSummaryDto>> GetSummary(int championId)
    {
        Champion? champion = await _db.Champions.FindAsync(championId);
        if (champion == null)
            return NotFoundProblem($"Champion {championId} was not found.");

        List<Build> builds = await _db.Builds
            .Where(b => b.ChampionId == championId)
            .Include(b => b.Author)
            .Include(b => b.Comments)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        int commentCount = 0;
        var buildsPerPosition = new Dictionary<string, int>();
        var latestBuilds = new List<BuildDto>();
        foreach (Build build in builds)
        {
            commentCount += build.Comments.Count;
            buildsPerPosition[build.Position] = buildsPerPosition.GetValueOrDefault(build.Position) + 1;
            if (latestBuilds.Count < 3)
                latestBuilds.Add(Mapping.ToDto(build));
        }

        var links = new List<LinkDto>
        {
            new($"/api/v1/champions/{championId}/summary", "self", "GET"),
            new($"/api/v1/champions/{championId}", "champion", "GET"),
            new($"/api/v1/champions/{championId}/builds", "builds", "GET"),
        };
        return Ok(new ChampionSummaryDto(Mapping.ToDto(champion), builds.Count, commentCount,
            buildsPerPosition, latestBuilds, links));
    }

    /// <summary>Create a champion. Admin only.</summary>
    /// <response code="401">Not logged in, or the token is missing, invalid or expired.</response>
    /// <response code="403">Logged in, but not allowed (wrong role, or not the author).</response>
    /// <response code="201">Created. The Location header points to the new resource.</response>
    /// <response code="400">The body is not valid JSON or has wrong value types.</response>
    /// <response code="422">The JSON is valid, but the values break validation rules.</response>
    /// <response code="409">Conflict: that name, username or email already exists.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ChampionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ChampionDto>> Create(ChampionRequest request)
    {
        string name = request.Name.Trim();
        if (await _db.Champions.AnyAsync(c => c.Name == name))
            return ConflictProblem($"A champion named '{name}' already exists.");

        var champion = new Champion
        {
            Name = name,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            ImageUrl = request.ImageUrl.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
        _db.Champions.Add(champion);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOne), new { championId = champion.Id }, Mapping.ToDto(champion));
    }

    /// <summary>Update a champion. Admin only.</summary>
    /// <response code="401">Not logged in, or the token is missing, invalid or expired.</response>
    /// <response code="403">Logged in, but not allowed (wrong role, or not the author).</response>
    /// <response code="200">Success.</response>
    /// <response code="400">The body is not valid JSON or has wrong value types.</response>
    /// <response code="404">Not found, or it does not belong to the parent in the URL.</response>
    /// <response code="409">Conflict: that name, username or email already exists.</response>
    /// <response code="422">The JSON is valid, but the values break validation rules.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{championId:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ChampionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ChampionDto>> Update(int championId, ChampionRequest request)
    {
        Champion? champion = await _db.Champions.FindAsync(championId);
        if (champion == null)
            return NotFoundProblem($"Champion {championId} was not found.");

        string name = request.Name.Trim();
        if (await _db.Champions.AnyAsync(c => c.Name == name && c.Id != championId))
            return ConflictProblem($"A champion named '{name}' already exists.");

        champion.Name = name;
        champion.Title = request.Title.Trim();
        champion.Description = request.Description.Trim();
        champion.ImageUrl = request.ImageUrl.Trim();
        await _db.SaveChangesAsync();

        return Ok(Mapping.ToDto(champion));
    }

    /// <summary>Delete a champion, together with its builds and their comments. Admin only.</summary>
    /// <response code="401">Not logged in, or the token is missing, invalid or expired.</response>
    /// <response code="403">Logged in, but not allowed (wrong role, or not the author).</response>
    /// <response code="204">Done, no response body.</response>
    /// <response code="404">Not found, or it does not belong to the parent in the URL.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{championId:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int championId)
    {
        Champion? champion = await _db.Champions.FindAsync(championId);
        if (champion == null)
            return NotFoundProblem($"Champion {championId} was not found.");

        _db.Champions.Remove(champion);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
