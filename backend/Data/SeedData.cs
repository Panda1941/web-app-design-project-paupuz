using LolBuilds.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace LolBuilds.Api.Data;

// Fills an empty database with meaningful demo data.
public static class SeedData
{
    public static void Seed(AppDbContext db)
    {
        if (db.Champions.Any())
            return; // already seeded

        DateTime now = DateTime.UtcNow;
        var hasher = new PasswordHasher<User>();

        // ---------- Users (all demo passwords: Password123!) ----------
        var admin = new User { Username = "admin", Email = "admin@lolbuilds.dev", Role = Roles.Admin, CreatedAt = now.AddDays(-60) };
        var fakerFan = new User { Username = "faker_fan", Email = "faker_fan@lolbuilds.dev", Role = Roles.User, CreatedAt = now.AddDays(-50) };
        var jungleDiff = new User { Username = "jungle_diff", Email = "jungle_diff@lolbuilds.dev", Role = Roles.User, CreatedAt = now.AddDays(-45) };
        var supportMain = new User { Username = "support_main", Email = "support_main@lolbuilds.dev", Role = Roles.User, CreatedAt = now.AddDays(-40) };
        var users = new List<User> { admin, fakerFan, jungleDiff, supportMain };
        foreach (User user in users)
            user.PasswordHash = hasher.HashPassword(user, "Password123!");
        db.Users.AddRange(users);
        db.SaveChanges(); // admin gets id 1, faker_fan id 2 (used as the lab 1 default author)

        // ---------- Champions ----------
        Champion ahri = Champion("Ahri", "the Nine-Tailed Fox",
            "Innately connected to the magic of the spirit realm, Ahri is a fox-like vastaya who can manipulate her prey's emotions and consume their essence.", now.AddDays(-30));
        Champion jinx = Champion("Jinx", "the Loose Cannon",
            "An unhinged and impulsive criminal from the undercity, Jinx is haunted by the consequences of her past, but that doesn't stop her from bringing her own chaos to Piltover and Zaun.", now.AddDays(-30));
        Champion yasuo = Champion("Yasuo", "the Unforgiven",
            "An Ionian of deep resolve, Yasuo is an agile swordsman who wields the air itself against his enemies.", now.AddDays(-29));
        Champion leeSin = Champion("Lee Sin", "the Blind Monk",
            "A master of Ionia's ancient martial arts, Lee Sin is a principled fighter who channels the essence of the dragon spirit to face any challenge.", now.AddDays(-29));
        Champion malphite = Champion("Malphite", "Shard of the Monolith",
            "A massive creature of living stone, Malphite struggles to impose blessed order on a chaotic world.", now.AddDays(-28));
        Champion lux = Champion("Lux", "the Lady of Luminosity",
            "Luxanna Crownguard hails from Demacia, an insular realm where magical abilities are viewed with fear and suspicion. Able to bend light to her will, she grew up dreading discovery.", now.AddDays(-28));
        Champion darius = Champion("Darius", "the Hand of Noxus",
            "There is no greater symbol of Noxian might than Darius, the nation's most feared and battle-hardened commander.", now.AddDays(-27));
        Champion thresh = Champion("Thresh", "the Chain Warden",
            "Sadistic and cunning, Thresh is an ambitious and restless specter of the Shadow Isles who collects the souls of his victims.", now.AddDays(-27));
        db.Champions.AddRange(ahri, jinx, yasuo, leeSin, malphite, lux, darius, thresh);
        db.SaveChanges();

        // ---------- Builds ----------
        Build ahriBurst = Build(ahri, fakerFan, "Electrocute burst mid", "Mid",
            "Standard burst setup. Look for charm into full combo after level 6 and roam to side lanes with your ultimate.",
            new() { 3020, 3100, 4645, 3157, 3089, 3135 }, "SummonerFlash", "SummonerDot",
            "Domination", "Electrocute", "Sorcery", now.AddDays(-20));
        Build ahriSafe = Build(ahri, supportMain, "Safe Aery poke into teamfights", "Mid",
            "For lanes against assassins. Poke with Q and keep charm for self-peel instead of going in.",
            new() { 3158, 3118, 3157, 4645, 3089 }, "SummonerFlash", "SummonerTeleport",
            "Sorcery", "Summon Aery", "Inspiration", now.AddDays(-12));
        Build jinxCrit = Build(jinx, fakerFan, "Lethal Tempo hypercarry", "Bot",
            "Scale safely in lane, then use rocket range in fights. Resets on kills make late-game fights snowball.",
            new() { 3006, 3032, 3031, 3046, 3036, 3072 }, "SummonerFlash", "SummonerHeal",
            "Precision", "Lethal Tempo", "Inspiration", now.AddDays(-18));
        Build yasuoCrit = Build(yasuo, jungleDiff, "Double-crit Yasuo", "Mid",
            "Rush Statikk Shiv for wave clear, then stack crit. Two crit items give 100% crit with Yasuo's passive.",
            new() { 3006, 3087, 3031, 3072, 6333, 3026 }, "SummonerFlash", "SummonerDot",
            "Precision", "Lethal Tempo", "Resolve", now.AddDays(-16));
        Build leeSinBruiser = Build(leeSin, jungleDiff, "Early pressure Lee Sin", "Jungle",
            "Full clear to a level 3 gank. Conqueror and Eclipse give strong skirmishes before 20 minutes.",
            new() { 3111, 6692, 3071, 3053, 6333 }, "SummonerFlash", "SummonerSmite",
            "Precision", "Conqueror", "Inspiration", now.AddDays(-15));
        Build malphiteTank = Build(malphite, supportMain, "Anti-AD tank top", "Top",
            "Into AD-heavy lanes. Armor scales your Q and W, and the ultimate stays your main win condition.",
            new() { 3047, 3075, 3068, 3084, 6665 }, "SummonerFlash", "SummonerTeleport",
            "Resolve", "Grasp of the Undying", "Sorcery", now.AddDays(-14));
        Build malphiteAp = Build(malphite, fakerFan, "AP one-shot Malphite", "Mid",
            "Troll pick that works: ultimate plus Q deletes squishies. Very weak if you fall behind.",
            new() { 3020, 4645, 3089, 3157, 3135 }, "SummonerFlash", "SummonerDot",
            "Sorcery", "Arcane Comet", "Domination", now.AddDays(-9));
        Build luxSupport = Build(lux, supportMain, "Poke support Lux", "Support",
            "Keep the enemy bot lane low with E and lock down engages with Q. Shield your carry in fights.",
            new() { 3158, 3118, 4645, 3089, 3157 }, "SummonerFlash", "SummonerDot",
            "Sorcery", "Arcane Comet", "Inspiration", now.AddDays(-11));
        Build dariusTop = Build(darius, jungleDiff, "Conqueror lane bully", "Top",
            "Win lane with passive stacks and pull with E when the enemy tries to walk away.",
            new() { 3047, 6610, 3053, 6333, 3071 }, "SummonerFlash", "SummonerHaste",
            "Precision", "Conqueror", "Resolve", now.AddDays(-8));
        Build threshSupport = Build(thresh, supportMain, "Playmaker Thresh", "Support",
            "Look for hooks from fog of war and lantern your jungler in. Aftershock lets you survive the engage.",
            new() { 3111, 3869, 3190, 3075 }, "SummonerFlash", "SummonerBoost",
            "Resolve", "Aftershock", "Inspiration", now.AddDays(-6));
        db.Builds.AddRange(ahriBurst, ahriSafe, jinxCrit, yasuoCrit, leeSinBruiser, malphiteTank,
            malphiteAp, luxSupport, dariusTop, threshSupport);
        db.SaveChanges();

        // ---------- Comments ----------
        db.Comments.AddRange(
            Comment(ahriBurst, jungleDiff, "Works really well, my roams got way stronger with this.", now.AddDays(-19)),
            Comment(ahriBurst, supportMain, "Would you go Zhonya's earlier against Zed?", now.AddDays(-18)),
            Comment(ahriBurst, fakerFan, "Yes, against Zed or Talon get Zhonya's second.", now.AddDays(-18)),
            Comment(ahriSafe, fakerFan, "Good for blind pick when you don't know the enemy mid.", now.AddDays(-11)),
            Comment(jinxCrit, supportMain, "Lethal Tempo feels much better than Fleet on Jinx right now.", now.AddDays(-17)),
            Comment(jinxCrit, jungleDiff, "Need a peel support for this or you get dived every fight.", now.AddDays(-16)),
            Comment(yasuoCrit, fakerFan, "Shiv first item is so good for wave clear.", now.AddDays(-15)),
            Comment(leeSinBruiser, supportMain, "Eclipse first gives so much early damage, nice.", now.AddDays(-14)),
            Comment(malphiteTank, jungleDiff, "Thornmail into Yasuo top is a free lane.", now.AddDays(-13)),
            Comment(malphiteAp, supportMain, "Tried this in ranked and one-shot their ADC at 15 minutes.", now.AddDays(-8)),
            Comment(luxSupport, fakerFan, "Comet poke in lane is unreal.", now.AddDays(-10)),
            Comment(dariusTop, supportMain, "Ghost or Flash second? I keep dying without Flash.", now.AddDays(-7)),
            Comment(threshSupport, jungleDiff, "Lantern ganks bot are the best, thanks for the build.", now.AddDays(-5)));
        db.SaveChanges();
    }

    private static Champion Champion(string name, string title, string description, DateTime createdAt)
    {
        string imageKey = name.Replace(" ", "");
        return new Champion
        {
            Name = name,
            Title = title,
            Description = description,
            ImageUrl = $"/images/champions/{imageKey}.png",
            CreatedAt = createdAt,
        };
    }

    private static Build Build(Champion champion, User author, string title, string position, string description,
        List<int> items, string spell1, string spell2, string primaryTree, string keystone, string secondaryTree,
        DateTime createdAt)
    {
        return new Build
        {
            ChampionId = champion.Id,
            AuthorId = author.Id,
            Title = title,
            Position = position,
            Description = description,
            Items = items,
            SummonerSpells = new() { spell1, spell2 },
            PrimaryRuneTree = primaryTree,
            Keystone = keystone,
            SecondaryRuneTree = secondaryTree,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };
    }

    private static Comment Comment(Build build, User author, string text, DateTime createdAt)
    {
        return new Comment
        {
            BuildId = build.Id,
            AuthorId = author.Id,
            Text = text,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };
    }
}
