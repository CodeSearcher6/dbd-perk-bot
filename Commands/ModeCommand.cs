using Discord.WebSocket;

namespace DBDPerkBot;

public class ModeCommand
{
    private readonly UserSettingsService _users;
    private readonly LocaleService _loc;

    private static readonly HashSet<string> ValidModes = new()
    {
        "random", "streamer", "solo", "meta", "troll", "forteams", "advanced", "normal"
    };

    public ModeCommand(UserSettingsService users, LocaleService loc)
    {
        _users = users;
        _loc = loc;
    }

    public async Task Handle(SocketMessage msg)
    {
        if (!CommandParser.TryMatch(msg.Content, "mode", out var args)) return;

        var parts = string.IsNullOrWhiteSpace(args)
            ? Array.Empty<string>()
            : args.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        var lang = _users.GetLang(msg.Author.Id);

        if (parts.Length == 0)
        {
            var current = _users.GetMode(msg.Author.Id);
            await msg.Channel.SendMessageAsync(_loc.T(lang, "ModeCurrent", current));
            return;
        }

        var mode = parts[0].ToLowerInvariant();

        if (mode == "normal")
            mode = "random";

        if (!ValidModes.Contains(mode) && mode != "random")
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "ModeUsage"));
            return;
        }

        _users.SetMode(msg.Author.Id, mode);
        await msg.Channel.SendMessageAsync(_loc.T(lang, "ModeSet", mode));
    }

}
