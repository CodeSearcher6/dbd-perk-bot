using Discord.WebSocket;

namespace DBDPerkBot.Commands;

public class ResetPerksCommand
{
    private readonly UserSettingsService _users;
    private readonly LocaleService _loc;

    public ResetPerksCommand(UserSettingsService users, LocaleService loc)
    {
        _users = users;
        _loc = loc;
    }

    public async Task Handle(SocketMessage msg)
    {
        if (!msg.Content.StartsWith("!resetperks")) return;

        var lang = _users.GetLang(msg.Author.Id);
        var user = _users.GetSettings(msg.Author.Id);

        user.OwnedPerks.Clear();
        user.MissingPerks.Clear();
        user.LastRolledPerks.Clear();

        await _users.SaveAsync(msg.Author.Id);

        await msg.Channel.SendMessageAsync(
            _loc.T(lang, "ResetPerksDone")
        );
    }
}
