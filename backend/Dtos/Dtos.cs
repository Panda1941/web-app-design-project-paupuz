using System.ComponentModel.DataAnnotations;

namespace LolBuilds.Api.Dtos;

// ---------- Shared ----------

// Hypermedia link: tells the client which related URLs and actions are available.
public record LinkDto(string Href, string Rel, string Method);

public record PagedResponse<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    List<LinkDto> Links);

// ---------- Champion ----------

public record ChampionDto(
    int Id,
    string Name,
    string Title,
    string Description,
    string ImageUrl,
    DateTime CreatedAt,
    List<LinkDto> Links);

public class ChampionRequest
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = "";

    [Required, StringLength(80, MinimumLength = 2)]
    public string Title { get; set; } = "";

    [Required, StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = "";

    [Required, StringLength(300)]
    public string ImageUrl { get; set; } = "";
}

// Aggregate resource built from several entities (champion + its builds + their comments).
public record ChampionSummaryDto(
    ChampionDto Champion,
    int BuildCount,
    int CommentCount,
    Dictionary<string, int> BuildsPerPosition,
    List<BuildDto> LatestBuilds,
    List<LinkDto> Links);

// ---------- Build ----------

public record BuildDto(
    int Id,
    int ChampionId,
    string ChampionName,
    string Title,
    string Description,
    string Position,
    List<int> Items,
    List<string> SummonerSpells,
    string PrimaryRuneTree,
    string Keystone,
    string SecondaryRuneTree,
    int AuthorId,
    string AuthorUsername,
    int CommentCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<LinkDto> Links);

public class BuildRequest
{
    [Required, StringLength(80, MinimumLength = 3)]
    public string Title { get; set; } = "";

    [Required, StringLength(4000, MinimumLength = 10)]
    public string Description { get; set; } = "";

    [Required, AllowedValues("Top", "Jungle", "Mid", "Bot", "Support")]
    public string Position { get; set; } = "";

    [Required, MinLength(1), MaxLength(6)]
    public List<int> Items { get; set; } = new();

    [Required, MinLength(2), MaxLength(2)]
    public List<string> SummonerSpells { get; set; } = new();

    [Required]
    public string PrimaryRuneTree { get; set; } = "";

    [Required]
    public string Keystone { get; set; } = "";

    [Required]
    public string SecondaryRuneTree { get; set; } = "";
}

// ---------- Comment ----------

public record CommentDto(
    int Id,
    int BuildId,
    string Text,
    int AuthorId,
    string AuthorUsername,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<LinkDto> Links);

public class CommentRequest
{
    [Required, StringLength(1000, MinimumLength = 2)]
    public string Text { get; set; } = "";
}
