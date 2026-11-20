using LolBuilds.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace LolBuilds.Api.Data;

// Fills an empty database with meaningful demo data.
// Item IDs, spells and runes are real values from Riot's Data Dragon.
public static class SeedData
{
    public static void Seed(AppDbContext db)
    {
        if (db.Champions.Any())
            return; // already seeded

        DateTime now = DateTime.UtcNow;

        // ---------- Users (all demo passwords: Password123!) ----------
        User admin = User("admin", Roles.Admin, now.AddDays(-60));
        User fakerFan = User("faker_fan", Roles.User, now.AddDays(-50));
        User jungleDiff = User("jungle_diff", Roles.User, now.AddDays(-45));
        User supportMain = User("support_main", Roles.User, now.AddDays(-40));
        User topGapTom = User("top_gap_tom", Roles.User, now.AddDays(-38));
        User adcAndy = User("adc_andy", Roles.User, now.AddDays(-35));
        User wardWarden = User("ward_warden", Roles.User, now.AddDays(-33));
        User midOrFeed = User("mid_or_feed", Roles.User, now.AddDays(-30));
        var users = new List<User> { admin, fakerFan, jungleDiff, supportMain, topGapTom, adcAndy, wardWarden, midOrFeed };
        var hasher = new PasswordHasher<User>();
        foreach (User user in users)
            user.PasswordHash = hasher.HashPassword(user, "Password123!");
        db.Users.AddRange(users);
        db.SaveChanges();

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
        Champion garen = Champion("Garen", "the Might of Demacia",
            "A proud and noble warrior, Garen fights as one of the Dauntless Vanguard. He is popular among his fellows and respected well enough by his enemies.", now.AddDays(-26));
        Champion ezreal = Champion("Ezreal", "the Prodigal Explorer",
            "A dashing adventurer, unknowingly gifted in the magical arts, Ezreal raids long-lost catacombs, tangles with ancient curses and overcomes seemingly impossible odds with ease.", now.AddDays(-26));
        db.Champions.AddRange(ahri, jinx, yasuo, leeSin, malphite, lux, darius, thresh, garen, ezreal);
        db.SaveChanges();

        // ---------- Builds ----------
        // Items (in buy order): 3020 Sorcerer's Shoes, 3006 Berserker's, 3047 Steelcaps, 3111 Mercury's,
        // 3158 Ionian Boots, 3089 Rabadon's, 3157 Zhonya's, 4645 Shadowflame, 3135 Void Staff, ...
        Build ahriBurst = Build(ahri, fakerFan, "Electrocute burst mid", "Mid",
            "Standard burst setup. Look for charm into full combo after level 6 and roam to side lanes with your ultimate.",
            new() { 3020, 3100, 4645, 3157, 3089, 3135 }, "SummonerFlash", "SummonerDot",
            "Domination", "Electrocute", "Sorcery", now.AddDays(-20));
        Build ahriSafe = Build(ahri, supportMain, "Safe Aery poke into teamfights", "Mid",
            "For lanes against assassins. Poke with Q and keep charm for self-peel instead of going in.",
            new() { 3158, 3118, 3157, 4645, 3089 }, "SummonerFlash", "SummonerTeleport",
            "Sorcery", "Summon Aery", "Inspiration", now.AddDays(-12));
        Build ahriRoam = Build(ahri, midOrFeed, "Luden's roaming Ahri", "Mid",
            "Luden's gives the fastest wave clear. Shove with Q, then use your ultimate charges to roam bot or into the jungle before the enemy mid can follow.",
            new() { 3020, 6655, 4645, 3157, 3135 }, "SummonerFlash", "SummonerDot",
            "Domination", "Electrocute", "Inspiration", now.AddDays(-4));

        Build jinxCrit = Build(jinx, fakerFan, "Lethal Tempo hypercarry", "Bot",
            "Scale safely in lane, then use rocket range in fights. Resets on kills make late-game fights snowball.",
            new() { 3006, 3032, 3031, 3046, 3036, 3072 }, "SummonerFlash", "SummonerHeal",
            "Precision", "Lethal Tempo", "Inspiration", now.AddDays(-18));
        Build jinxOnHit = Build(jinx, adcAndy, "Rageblade on-hit Jinx", "Bot",
            "Great into tank-heavy teams. Guinsoo's and Runaan's let minigun shred frontliners, Blade of the Ruined King keeps you alive.",
            new() { 3006, 3124, 3085, 3153, 3091 }, "SummonerFlash", "SummonerHeal",
            "Precision", "Lethal Tempo", "Resolve", now.AddDays(-7));

        Build yasuoCrit = Build(yasuo, jungleDiff, "Double-crit Yasuo", "Mid",
            "Rush Statikk Shiv for wave clear, then stack crit. Two crit items give 100% crit with Yasuo's passive.",
            new() { 3006, 3087, 3031, 3072, 6333, 3026 }, "SummonerFlash", "SummonerDot",
            "Precision", "Lethal Tempo", "Resolve", now.AddDays(-16));
        Build yasuoTop = Build(yasuo, topGapTom, "Top lane Yasuo into melee matchups", "Top",
            "Play top when the enemy top laner is melee. Conqueror stacks fast with Q, and Shieldbow saves you from all-ins.",
            new() { 3006, 6675, 3031, 6673, 6333 }, "SummonerFlash", "SummonerTeleport",
            "Precision", "Conqueror", "Resolve", now.AddDays(-6));

        Build leeSinBruiser = Build(leeSin, jungleDiff, "Early pressure Lee Sin", "Jungle",
            "Full clear to a level 3 gank. Conqueror and Eclipse give strong skirmishes before 20 minutes.",
            new() { 3111, 6692, 3071, 3053, 6333 }, "SummonerFlash", "SummonerSmite",
            "Precision", "Conqueror", "Inspiration", now.AddDays(-15));
        Build leeSinTank = Build(leeSin, wardWarden, "Tanky Lee Sin for teams without engage", "Jungle",
            "When your team has no frontline. Sundered Sky and Spirit Visage keep you alive long enough to kick the carry into your team.",
            new() { 3047, 6610, 3053, 3071, 3065 }, "SummonerFlash", "SummonerSmite",
            "Precision", "Conqueror", "Resolve", now.AddDays(-3));

        Build malphiteTank = Build(malphite, supportMain, "Anti-AD tank top", "Top",
            "Into AD-heavy lanes. Armor scales your Q and W, and the ultimate stays your main win condition.",
            new() { 3047, 3075, 3068, 3084, 6665 }, "SummonerFlash", "SummonerTeleport",
            "Resolve", "Grasp of the Undying", "Sorcery", now.AddDays(-14));
        Build malphiteAp = Build(malphite, fakerFan, "AP one-shot Malphite", "Mid",
            "Troll pick that works: ultimate plus Q deletes squishies. Very weak if you fall behind.",
            new() { 3020, 4645, 3089, 3157, 3135 }, "SummonerFlash", "SummonerDot",
            "Sorcery", "Arcane Comet", "Domination", now.AddDays(-9));
        Build malphiteSupport = Build(malphite, wardWarden, "Engage support Malphite", "Support",
            "Pairs well with lane bullies. Ultimate onto the enemy bot lane at level 6 usually wins the lane, Locket and Zeke's help your team in fights.",
            new() { 3047, 3190, 3050, 3075 }, "SummonerFlash", "SummonerDot",
            "Resolve", "Aftershock", "Inspiration", now.AddDays(-5));

        Build luxSupport = Build(lux, supportMain, "Poke support Lux", "Support",
            "Keep the enemy bot lane low with E and lock down engages with Q. Shield your carry in fights.",
            new() { 3158, 3118, 4645, 3089, 3157 }, "SummonerFlash", "SummonerDot",
            "Sorcery", "Arcane Comet", "Inspiration", now.AddDays(-11));
        Build luxMid = Build(lux, midOrFeed, "Burst mid Lux", "Mid",
            "E, Q, then ultimate on the rooted target. Electrocute and Luden's make the combo lethal on squishies from level 6.",
            new() { 3020, 6655, 4645, 3089, 3135 }, "SummonerFlash", "SummonerTeleport",
            "Domination", "Electrocute", "Sorcery", now.AddDays(-8));

        Build dariusTop = Build(darius, jungleDiff, "Conqueror lane bully", "Top",
            "Win lane with passive stacks and pull with E when the enemy tries to walk away.",
            new() { 3047, 6610, 3053, 6333, 3071 }, "SummonerFlash", "SummonerHaste",
            "Precision", "Conqueror", "Resolve", now.AddDays(-10));
        Build dariusJungle = Build(darius, topGapTom, "Jungle Darius with Stridebreaker", "Jungle",
            "Fast clears with W resets. Stridebreaker slow plus Ghost makes ganks almost guaranteed once you hit level 3.",
            new() { 3111, 6631, 3053, 6333, 3742 }, "SummonerHaste", "SummonerSmite",
            "Precision", "Conqueror", "Inspiration", now.AddDays(-2));

        Build threshSupport = Build(thresh, supportMain, "Playmaker Thresh", "Support",
            "Look for hooks from fog of war and lantern your jungler in. Aftershock lets you survive the engage.",
            new() { 3111, 3190, 3109, 3075 }, "SummonerFlash", "SummonerBoost",
            "Resolve", "Aftershock", "Inspiration", now.AddDays(-6));
        Build threshPeel = Build(thresh, wardWarden, "Guardian Thresh for scaling carries", "Support",
            "Protect a hypercarry instead of hunting picks. Guardian, Redemption and Mikael's keep your ADC alive in every fight.",
            new() { 3047, 3190, 3107, 3222 }, "SummonerFlash", "SummonerExhaust",
            "Resolve", "Guardian", "Inspiration", now.AddDays(-2));

        Build garenTop = Build(garen, topGapTom, "Classic Conqueror Garen", "Top",
            "Q silences, E spins, and the ultimate executes. Stridebreaker helps you stick to ranged champions.",
            new() { 3047, 6631, 3053, 6333, 3742 }, "SummonerFlash", "SummonerDot",
            "Precision", "Conqueror", "Resolve", now.AddDays(-13));
        Build garenLethality = Build(garen, adcAndy, "Lethality Garen", "Top",
            "Just for fun: Youmuu's and Serylda's turn every spin into a one-shot. Falls off if the enemy builds armor.",
            new() { 3047, 3142, 6694, 3814, 3036 }, "SummonerFlash", "SummonerDot",
            "Precision", "Press the Attack", "Domination", now.AddDays(-3));

        Build ezrealBot = Build(ezreal, adcAndy, "Manamune Trinity Ezreal", "Bot",
            "Stack Manamune with Q from the start. Trinity Force and Shojin make your Q hit hard while staying safe at range.",
            new() { 3158, 3004, 3078, 3161, 3036 }, "SummonerFlash", "SummonerHeal",
            "Inspiration", "First Strike", "Sorcery", now.AddDays(-13));
        Build ezrealAp = Build(ezreal, midOrFeed, "AP Ezreal mid", "Mid",
            "Lich Bane and Luden's turn every Q and W combo into burst. Better into immobile mid laners.",
            new() { 3020, 3100, 6655, 3089, 3135 }, "SummonerFlash", "SummonerTeleport",
            "Sorcery", "Arcane Comet", "Inspiration", now.AddDays(-5));

        db.Builds.AddRange(ahriBurst, ahriSafe, ahriRoam, jinxCrit, jinxOnHit, yasuoCrit, yasuoTop,
            leeSinBruiser, leeSinTank, malphiteTank, malphiteAp, malphiteSupport, luxSupport, luxMid,
            dariusTop, dariusJungle, threshSupport, threshPeel, garenTop, garenLethality, ezrealBot, ezrealAp);
        db.SaveChanges();

        // ---------- Comments ----------
        db.Comments.AddRange(
            Comment(ahriBurst, jungleDiff, "Works really well, my roams got way stronger with this.", now.AddDays(-19)),
            Comment(ahriBurst, supportMain, "Would you go Zhonya's earlier against Zed?", now.AddDays(-18)),
            Comment(ahriBurst, fakerFan, "Yes, against Zed or Talon get Zhonya's second.", now.AddDays(-18)),
            Comment(ahriBurst, midOrFeed, "Lich Bane first feels a bit slow on wave clear, I prefer Luden's.", now.AddDays(-5)),
            Comment(ahriSafe, fakerFan, "Good for blind pick when you don't know the enemy mid.", now.AddDays(-11)),
            Comment(ahriSafe, wardWarden, "Aery poke with Ahri is so annoying to lane against.", now.AddDays(-9)),
            Comment(ahriRoam, jungleDiff, "As a jungler I love an Ahri that actually roams. Thanks!", now.AddDays(-3)),
            Comment(ahriRoam, fakerFan, "Inspiration secondary for boots and biscuits is a nice touch.", now.AddDays(-2)),
            Comment(jinxCrit, supportMain, "Lethal Tempo feels much better than Fleet on Jinx right now.", now.AddDays(-17)),
            Comment(jinxCrit, jungleDiff, "Need a peel support for this or you get dived every fight.", now.AddDays(-16)),
            Comment(jinxCrit, adcAndy, "Bloodthirster last is good, the shield saves you a lot.", now.AddDays(-8)),
            Comment(jinxOnHit, fakerFan, "Tried it against Malphite and Ornn, minigun melted them.", now.AddDays(-6)),
            Comment(jinxOnHit, supportMain, "Feels weak before Guinsoo's is done, play safe until then.", now.AddDays(-5)),
            Comment(yasuoCrit, fakerFan, "Shiv first item is so good for wave clear.", now.AddDays(-15)),
            Comment(yasuoCrit, topGapTom, "Guardian Angel last is smart, Yasuo always gets focused.", now.AddDays(-12)),
            Comment(yasuoTop, jungleDiff, "Shieldbow on Yasuo top is underrated.", now.AddDays(-5)),
            Comment(yasuoTop, adcAndy, "What do you do into Malphite though?", now.AddDays(-4)),
            Comment(yasuoTop, topGapTom, "Dodge or go mid, that matchup is really hard.", now.AddDays(-4)),
            Comment(leeSinBruiser, supportMain, "Eclipse first gives so much early damage, nice.", now.AddDays(-14)),
            Comment(leeSinBruiser, wardWarden, "Remember to ward hop to the enemy jungler's buff at level 2.", now.AddDays(-10)),
            Comment(leeSinTank, jungleDiff, "Spirit Visage on Lee is strange but it makes sense here.", now.AddDays(-2)),
            Comment(malphiteTank, jungleDiff, "Thornmail into Yasuo top is a free lane.", now.AddDays(-13)),
            Comment(malphiteTank, topGapTom, "Heartsteel stacks get huge if you farm well.", now.AddDays(-9)),
            Comment(malphiteAp, supportMain, "Tried this in ranked and one-shot their ADC at 15 minutes.", now.AddDays(-8)),
            Comment(malphiteAp, midOrFeed, "Arcane Comet with Q is so much poke in lane.", now.AddDays(-6)),
            Comment(malphiteSupport, adcAndy, "Played with a Malphite support yesterday, the level 6 all-in was crazy.", now.AddDays(-4)),
            Comment(luxSupport, fakerFan, "Comet poke in lane is unreal.", now.AddDays(-10)),
            Comment(luxSupport, adcAndy, "Please shield me with W in fights, not just poke!", now.AddDays(-7)),
            Comment(luxSupport, supportMain, "Fair point, W on your carry is part of the plan.", now.AddDays(-7)),
            Comment(luxMid, wardWarden, "Luden's on Lux mid is my favourite build now.", now.AddDays(-6)),
            Comment(dariusTop, supportMain, "Ghost or Flash second? I keep dying without Flash.", now.AddDays(-9)),
            Comment(dariusTop, jungleDiff, "Flash plus Ghost. Teleport is fine too if your team needs it.", now.AddDays(-9)),
            Comment(dariusTop, topGapTom, "Sundered Sky first makes trades so much better.", now.AddDays(-5)),
            Comment(dariusJungle, jungleDiff, "Did not expect jungle Darius to work this well.", now.AddDays(-1)),
            Comment(threshSupport, jungleDiff, "Lantern ganks bot are the best, thanks for the build.", now.AddDays(-5)),
            Comment(threshSupport, adcAndy, "Knight's Vow on Thresh feels great with an immobile ADC.", now.AddDays(-4)),
            Comment(threshPeel, supportMain, "Guardian Thresh is very good with Jinx or Kog'Maw.", now.AddDays(-1)),
            Comment(garenTop, jungleDiff, "Simple and strong, good for new players.", now.AddDays(-12)),
            Comment(garenTop, fakerFan, "Stridebreaker into Teemo is a must.", now.AddDays(-8)),
            Comment(garenLethality, topGapTom, "This is ridiculous and I love it.", now.AddDays(-2)),
            Comment(garenLethality, supportMain, "Falls off hard once they buy Plated Steelcaps.", now.AddDays(-1)),
            Comment(ezrealBot, fakerFan, "Manamune stacks so fast with Q, good build.", now.AddDays(-12)),
            Comment(ezrealBot, wardWarden, "First Strike gold adds up quickly on Ezreal.", now.AddDays(-7)),
            Comment(ezrealAp, adcAndy, "AP Ezreal mid is a fun change from bot lane.", now.AddDays(-4)),
            Comment(ezrealAp, jungleDiff, "Hard to play into assassins, but the burst is huge.", now.AddDays(-3)));
        db.SaveChanges();
    }

    private static User User(string username, string role, DateTime createdAt)
    {
        return new User
        {
            Username = username,
            Email = $"{username}@lolbuilds.dev",
            Role = role,
            CreatedAt = createdAt,
        };
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
