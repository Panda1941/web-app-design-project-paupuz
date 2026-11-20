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
    /// <example>Garen</example>
    [Required, StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = "";

    /// <example>the Might of Demacia</example>
    [Required, StringLength(80, MinimumLength = 2)]
    public string Title { get; set; } = "";

    /// <example>A proud and noble warrior who fights as one of the Dauntless Vanguard.</example>
    [Required, StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = "";

    // A normal URL/path, or an uploaded picture stored as a data URL ("data:image/jpeg;base64,...").
    // The frontend shrinks uploads to 128x128, so 100 000 characters is plenty.
    /// <summary>A URL/path, or an uploaded picture as a data URL.</summary>
    /// <example>/images/champions/Garen.png</example>
    [Required, StringLength(100_000)]
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
    /// <example>Electrocute burst mid</example>
    [Required, StringLength(80, MinimumLength = 3)]
    public string Title { get; set; } = "";

    /// <example>Look for charm into full combo after level 6 and roam with your ultimate.</example>
    [Required, StringLength(4000, MinimumLength = 10)]
    public string Description { get; set; } = "";

    /// <summary>Top, Jungle, Mid, Bot or Support.</summary>
    /// <example>Mid</example>
    [Required, AllowedValues("Top", "Jungle", "Mid", "Bot", "Support")]
    public string Position { get; set; } = "";

    /// <summary>1-6 Data Dragon item IDs in buy order.</summary>
    /// <example>[3020, 3100, 4645, 3157, 3089, 3135]</example>
    [Required, MinLength(1), MaxLength(6)]
    public List<int> Items { get; set; } = new();

    /// <summary>Exactly 2 different spells: SummonerFlash, SummonerDot, SummonerTeleport, SummonerHeal, SummonerBarrier, SummonerExhaust, SummonerHaste, SummonerBoost, SummonerSmite.</summary>
    /// <example>["SummonerFlash", "SummonerDot"]</example>
    [Required, MinLength(2), MaxLength(2)]
    public List<string> SummonerSpells { get; set; } = new();

    /// <summary>Precision, Domination, Sorcery, Resolve or Inspiration.</summary>
    /// <example>Domination</example>
    [Required]
    public string PrimaryRuneTree { get; set; } = "";

    /// <summary>Must belong to the primary rune tree.</summary>
    /// <example>Electrocute</example>
    [Required]
    public string Keystone { get; set; } = "";

    /// <summary>Any tree except the primary one.</summary>
    /// <example>Sorcery</example>
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
    /// <example>Works really well, my roams got way stronger with this.</example>
    [Required, StringLength(1000, MinimumLength = 2)]
    public string Text { get; set; } = "";
}
