using DBDPerkBot.Models;

namespace DBDPerkBot;

public static class ModeTitles
{
    private static readonly Dictionary<string, string> _titles = new()
    {
        { "random", "Random Survivor Build" },
        { "streamer", "Streamer Build" },
        { "solo", "Solo Survivor Build" },
        { "forteams", "Team Survivor Build" },
        { "advanced", "Advanced Survivor Build" },
        { "meta", "Meta Survivor Build" },
        { "troll", "Troll Survivor Build" }
    };

    public static string Get(string mode)
    {
        mode = mode.ToLower();
        return _titles.TryGetValue(mode, out var title)
            ? title
            : "Random Survivor Build";
    }
}
