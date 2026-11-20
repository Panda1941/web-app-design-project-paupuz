using LolBuilds.Api.Models;

namespace LolBuilds.Api.Dtos;

// Converts database entities into response DTOs and adds hypermedia links.
public static class Mapping
{
    public static ChampionDto ToDto(Champion c)
    {
        string self = $"/api/champions/{c.Id}";
        var links = new List<LinkDto>
        {
            new(self, "self", "GET"),
            new(self, "update", "PUT"),
            new(self, "delete", "DELETE"),
            new($"{self}/builds", "builds", "GET"),
            new($"{self}/builds", "create-build", "POST"),
            new($"{self}/summary", "summary", "GET"),
        };
        return new ChampionDto(c.Id, c.Name, c.Title, c.Description, c.ImageUrl, c.CreatedAt, links);
    }

    // The build must be loaded with its Champion, Author and Comments.
    public static BuildDto ToDto(Build b)
    {
        string self = $"/api/champions/{b.ChampionId}/builds/{b.Id}";
        var links = new List<LinkDto>
        {
            new(self, "self", "GET"),
            new(self, "update", "PUT"),
            new(self, "delete", "DELETE"),
            new($"/api/champions/{b.ChampionId}", "champion", "GET"),
            new($"{self}/comments", "comments", "GET"),
            new($"{self}/comments", "create-comment", "POST"),
        };
        return new BuildDto(
            b.Id, b.ChampionId, b.Champion?.Name ?? "", b.Title, b.Description, b.Position,
            b.Items, b.SummonerSpells, b.PrimaryRuneTree, b.Keystone, b.SecondaryRuneTree,
            b.AuthorId, b.Author?.Username ?? "", b.Comments.Count,
            b.CreatedAt, b.UpdatedAt, links);
    }

    // The comment must be loaded with its Author.
    public static CommentDto ToDto(Comment c, int championId)
    {
        string buildUrl = $"/api/champions/{championId}/builds/{c.BuildId}";
        string self = $"{buildUrl}/comments/{c.Id}";
        var links = new List<LinkDto>
        {
            new(self, "self", "GET"),
            new(self, "update", "PUT"),
            new(self, "delete", "DELETE"),
            new(buildUrl, "build", "GET"),
        };
        return new CommentDto(c.Id, c.BuildId, c.Text, c.AuthorId, c.Author?.Username ?? "",
            c.CreatedAt, c.UpdatedAt, links);
    }

    // Links for a paged list: self, plus previous/next pages when they exist.
    public static List<LinkDto> PageLinks(string basePath, string extraQuery, int page, int pageSize, int totalPages)
    {
        var links = new List<LinkDto>
        {
            new($"{basePath}?page={page}&pageSize={pageSize}{extraQuery}", "self", "GET"),
            new(basePath, "create", "POST"),
        };
        if (page > 1)
            links.Add(new($"{basePath}?page={page - 1}&pageSize={pageSize}{extraQuery}", "previous", "GET"));
        if (page < totalPages)
            links.Add(new($"{basePath}?page={page + 1}&pageSize={pageSize}{extraQuery}", "next", "GET"));
        return links;
    }
}
