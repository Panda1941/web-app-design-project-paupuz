namespace LolBuilds.Api.Data;

// Fixed catalogue of summoner spells and runes (from Riot's Data Dragon).
// Used to validate builds. Items are only checked for being positive IDs.
public static class GameData
{
    public static readonly string[] SummonerSpells =
    {
        "SummonerBarrier", "SummonerBoost", "SummonerDot", "SummonerExhaust", "SummonerFlash",
        "SummonerHaste", "SummonerHeal", "SummonerSmite", "SummonerTeleport"
    };

    // Rune tree -> keystones in that tree.
    public static readonly Dictionary<string, string[]> RuneTrees = new()
    {
        ["Precision"] = new[] { "Press the Attack", "Lethal Tempo", "Fleet Footwork", "Conqueror" },
        ["Domination"] = new[] { "Electrocute", "Dark Harvest", "Hail of Blades" },
        ["Sorcery"] = new[] { "Summon Aery", "Arcane Comet", "Stormraider's Surge", "Deathfire Touch" },
        ["Resolve"] = new[] { "Grasp of the Undying", "Aftershock", "Guardian" },
        ["Inspiration"] = new[] { "Glacial Augment", "Unsealed Spellbook", "First Strike" },
    };
}
