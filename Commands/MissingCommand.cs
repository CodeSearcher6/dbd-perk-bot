using Discord.WebSocket;

namespace DBDPerkBot.Commands;

public class MissingCommand
{
    private readonly UserSettingsService _users;
    private readonly LocaleService _loc;

    public MissingCommand(UserSettingsService users, LocaleService loc)
    {
        _users = users;
        _loc = loc;
    }

    public async Task Handle(SocketMessage msg)
    {
        if (!msg.Content.StartsWith("!missing")) return;

        var id = msg.Author.Id;
        var lang = _users.GetLang(id);
        var u = _users.GetSettings(id);

        if (u.MissingPerks.Count == 0)
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "MissingNone"));
            return;
        }

        var text = string.Join(", ", u.MissingPerks.OrderBy(x => x));

        await msg.Channel.SendMessageAsync(_loc.T(lang, "MissingList", text));
    }
}
