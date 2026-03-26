using Discord.WebSocket;

namespace DBDPerkBot.Commands;

public class LangCommand
{
    private readonly UserSettingsService _users;
    private readonly LocaleService _loc;

    private readonly string[] _supported = { "en", "ua", "fr" };

    public LangCommand(UserSettingsService users, LocaleService loc)
    {
        _users = users;
        _loc = loc;
    }

    public async Task Handle(SocketMessage msg)
    {
        if (!CommandParser.TryMatch(msg.Content, "lang", out var args)) return;

        var userId = msg.Author.Id;

        // ✅ Користувач просто хоче побачити поточну мову
        if (string.IsNullOrWhiteSpace(args))
        {
            var lang = _users.GetLang(userId);
            var langDisplay = lang switch
            {
                "ua" => "Українська",
                "fr" => "Français",
                _ => "English"
            };

            await msg.Channel.SendMessageAsync(
                _loc.T(lang, "CurrentLang", langDisplay)
            );

            return;
        }

        // ✅ Користувач хоче змінити мову
        var newLang = args.Trim().ToLowerInvariant(); 

        if (!_supported.Contains(newLang))
        {
            var cur = _users.GetLang(userId);
            await msg.Channel.SendMessageAsync(_loc.T(cur, "InvalidLang"));
            return;
        }

        _users.SetLang(userId, newLang);

        var newDisplay = newLang switch
        {
            "ua" => "Українська",
            "fr" => "Français",
            _ => "English"
        };

        await msg.Channel.SendMessageAsync(
            _loc.T(newLang, "LangChanged", newDisplay)
        );
    }
}
