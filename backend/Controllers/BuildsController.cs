using LolBuilds.Api.Data;
using LolBuilds.Api.Dtos;
using LolBuilds.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LolBuilds.Api.Controllers;

[Route("api/champions/{championId:int}/builds")]
public class BuildsController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public BuildsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Get a paged list of one champion's builds.</summary>
    /// <param name="championId">Champion ID.</param>
    /// <param name="position">Optional filter: Top, Jungle, Mid, Bot or Support.</param>
    /// <param name="page">Page number, starting at 1.</param>
    /// <param name="pageSize">Items per page (1-50).</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<BuildDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<BuildDto>>> GetAll(
        int championId, [FromQuery] string? position, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (!await _db.Champions.AnyAsync(c => c.Id == championId))
            return NotFoundProblem($"Champion {championId} was not found.");

        (page, pageSize) = NormalizePaging(page, pageSize);

        IQueryable<Build> query = _db.Builds.Where(b => b.ChampionId == championId);
        string extraQuery = "";
        if (!string.IsNullOrWhiteSpace(position))
        {
            query = query.Where(b => b.Position == position);
            extraQuery = $"&position={Uri.EscapeDataString(position)}";
        }

        int totalCount = await query.CountAsync();
        List<Build> builds = await query
            .Include(b => b.Champion)
            .Include(b => b.Author)
            .Include(b => b.Comments)
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<BuildDto>();
        foreach (Build build in builds)
            items.Add(Mapping.ToDto(build));

        int totalPages = TotalPages(totalCount, pageSize);
        var links = Mapping.PageLinks($"/api/champions/{championId}/builds", extraQuery, page, pageSize, totalPages);
        links.Add(new LinkDto($"/api/champions/{championId}", "champion", "GET"));
        return Ok(new PagedResponse<BuildDto>(items, page, pageSize, totalCount, totalPages, links));
    }

    /// <summary>Get one build of a champion.</summary>
    [HttpGet("{buildId:int}")]
    [ProducesResponseType(typeof(BuildDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuildDto>> GetOne(int championId, int buildId)
    {
        Build? build = await FindBuild(championId, buildId);
        if (build == null)
            return NotFoundProblem($"Build {buildId} was not found for champion {championId}.");

        return Ok(Mapping.ToDto(build));
    }

    /// <summary>Create a build for a champion. Logged-in users only; the author is taken from the token.</summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BuildDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BuildDto>> Create(int championId, BuildRequest request)
    {
        if (!await _db.Champions.AnyAsync(c => c.Id == championId))
            return NotFoundProblem($"Champion {championId} was not found.");

        ObjectResult? error = ValidateBuild(request);
        if (error != null)
            return error;

        var build = new Build
        {
            ChampionId = championId,
            AuthorId = CurrentUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        CopyFromRequest(build, request);
        _db.Builds.Add(build);
        await _db.SaveChangesAsync();

        Build created = (await FindBuild(championId, build.Id))!;
        return CreatedAtAction(nameof(GetOne), new { championId, buildId = build.Id }, Mapping.ToDto(created));
    }

    /// <summary>Update a build. Only its author can do this.</summary>
    [Authorize]
    [HttpPut("{buildId:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BuildDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BuildDto>> Update(int championId, int buildId, BuildRequest request)
    {
        Build? build = await FindBuild(championId, buildId);
        if (build == null)
            return NotFoundProblem($"Build {buildId} was not found for champion {championId}.");
        if (!IsOwner(build.AuthorId))
            return ForbiddenProblem("You can only edit your own builds.");

        ObjectResult? error = ValidateBuild(request);
        if (error != null)
            return error;

        CopyFromRequest(build, request);
        build.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(Mapping.ToDto(build));
    }

    /// <summary>Delete a build, together with its comments. Its author or an admin can do this.</summary>
    [Authorize]
    [HttpDelete("{buildId:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int championId, int buildId)
    {
        Build? build = await FindBuild(championId, buildId);
        if (build == null)
            return NotFoundProblem($"Build {buildId} was not found for champion {championId}.");
        if (!IsOwner(build.AuthorId) && !IsAdmin)
            return ForbiddenProblem("You can only delete your own builds.");

        _db.Builds.Remove(build);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Finds a build only if it belongs to the given champion (URL scoping).
    private async Task<Build?> FindBuild(int championId, int buildId)
    {
        return await _db.Builds
            .Include(b => b.Champion)
            .Include(b => b.Author)
            .Include(b => b.Comments)
            .FirstOrDefaultAsync(b => b.Id == buildId && b.ChampionId == championId);
    }

    // Business rules that simple attributes can't express. Returns null when the build is valid.
    private ObjectResult? ValidateBuild(BuildRequest request)
    {
        foreach (int itemId in request.Items)
        {
            if (itemId <= 0)
                return Unprocessable("items", "Item IDs must be positive numbers.");
        }

        foreach (string spell in request.SummonerSpells)
        {
            if (!GameData.SummonerSpells.Contains(spell))
                return Unprocessable("summonerSpells", $"Unknown summoner spell '{spell}'.");
        }
        if (request.SummonerSpells[0] == request.SummonerSpells[1])
            return Unprocessable("summonerSpells", "The two summoner spells must be different.");

        if (!GameData.RuneTrees.ContainsKey(request.PrimaryRuneTree))
            return Unprocessable("primaryRuneTree", $"Unknown rune tree '{request.PrimaryRuneTree}'.");
        if (!GameData.RuneTrees.ContainsKey(request.SecondaryRuneTree))
            return Unprocessable("secondaryRuneTree", $"Unknown rune tree '{request.SecondaryRuneTree}'.");
        if (request.PrimaryRuneTree == request.SecondaryRuneTree)
            return Unprocessable("secondaryRuneTree", "The secondary rune tree must differ from the primary one.");
        if (!GameData.RuneTrees[request.PrimaryRuneTree].Contains(request.Keystone))
            return Unprocessable("keystone", $"'{request.Keystone}' is not a keystone of {request.PrimaryRuneTree}.");

        return null;
    }

    private static void CopyFromRequest(Build build, BuildRequest request)
    {
        build.Title = request.Title.Trim();
        build.Description = request.Description.Trim();
        build.Position = request.Position;
        build.Items = request.Items;
        build.SummonerSpells = request.SummonerSpells;
        build.PrimaryRuneTree = request.PrimaryRuneTree;
        build.Keystone = request.Keystone;
        build.SecondaryRuneTree = request.SecondaryRuneTree;
    }
}
