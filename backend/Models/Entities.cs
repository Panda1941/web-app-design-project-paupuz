namespace LolBuilds.Api.Models;

// The three domain objects form a one-to-many chain: Champion -> Build -> Comment.
// User is not a domain object; it is only used for authorship and (in lab 2) authentication.

public class Champion
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public List<Build> Builds { get; set; } = new();
}

public class Build
{
    public int Id { get; set; }
    public int ChampionId { get; set; }
    public Champion? Champion { get; set; }

    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Position { get; set; } = "";          // Top, Jungle, Mid, Bot, Support

    // Stored as JSON columns by EF Core, so no extra join tables are needed.
    public List<int> Items { get; set; } = new();          // Data Dragon item IDs, e.g. 3089
    public List<string> SummonerSpells { get; set; } = new(); // e.g. SummonerFlash
    public string PrimaryRuneTree { get; set; } = "";     // e.g. Domination
    public string Keystone { get; set; } = "";            // e.g. Electrocute
    public string SecondaryRuneTree { get; set; } = "";   // e.g. Sorcery

    public int AuthorId { get; set; }
    public User? Author { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<Comment> Comments { get; set; } = new();
}

public class Comment
{
    public int Id { get; set; }
    public int BuildId { get; set; }
    public Build? Build { get; set; }

    public string Text { get; set; } = "";

    public int AuthorId { get; set; }
    public User? Author { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = Roles.User;
    public DateTime CreatedAt { get; set; }
}

// Guest = no token. User and Admin are stored on the user and put into the JWT.
public static class Roles
{
    public const string User = "User";
    public const string Admin = "Admin";
}
