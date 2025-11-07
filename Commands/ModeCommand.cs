using Discord.WebSocket;

namespace DBDPerkBot;

public class ModeCommand
{
    private readonly UserSettingsService _users;
    private readonly LocaleService _loc;

    private static readonly HashSet<string> ValidModes = new()
    {
        "random", "streamer", "solo", "meta", "troll", "forteams", "advanced"
    };

    public ModeCommand(UserSettingsService users, LocaleService loc)
    {
        _users = users;
        _loc = loc;
    }

    public async Task Handle(SocketMessage msg)
    {
        if (!msg.Content.StartsWith("!mode")) return;

        var parts = msg.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        var lang = _users.GetLang(msg.Author.Id);

        // без аргументів → показує поточний режим
        if (parts.Length < 2)
        {
            var current = _users.GetMode(msg.Author.Id);
            var text = _loc.T(lang, "ModeCurrent", current); // наприклад: "Current mode: {0}"
            await msg.Channel.SendMessageAsync(text);
            return;
        }

        var mode = parts[1].ToLower();

        if (!ValidModes.Contains(mode))
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "ModeUsage"));
            return;
        }

        _users.SetMode(msg.Author.Id, mode);
        var confirm = _loc.T(lang, "ModeSet", mode); // наприклад: "Mode set to: {0}"
        await msg.Channel.SendMessageAsync(confirm);
    }
}
