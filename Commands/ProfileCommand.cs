using Discord.WebSocket;
using Microsoft.VisualBasic;

namespace DBDPerkBot.Commands;

public class ProfileCommand
{
    private readonly UserSettingsService _users;
    private readonly LocaleService _loc;

    public ProfileCommand(UserSettingsService users, LocaleService loc)
    {
        _users = users;
        _loc = loc;
    }

    public async Task Handle(SocketMessage msg)
    {
        if (!msg.Content.StartsWith("!profile")) return;

        var id = msg.Author.Id;
        var lang = _users.GetLang(id);

        var langDisplay = lang switch
        {
            "ua" => "Українська",
            "fr" => "Français",
            _ => "English"
        };

        var mode = _users.GetMode(id);
        var u = _users.GetSettings(id);

        string header = _loc.T(lang, "current_profile_header",
            $"Profile for {msg.Author.Username}"
        );

        string text = string.Format(
        _loc.T(lang, "ProfileCaption"),
        msg.Author.Username,
        u.MissingPerks.Count + u.MissingSurvivors.Count) + "\n\n" +
        string.Format(_loc.T(lang, "ProfileLang"), langDisplay) + "\n" +
        string.Format(_loc.T(lang, "ProfileMode"), mode) + "\n\n" +
        string.Format(_loc.T(lang, "ProfileOwnedSurvivors"), u.OwnedSurvivors.Count) + "\n" +
        string.Format(_loc.T(lang, "ProfileMissingSurvivors"), u.MissingSurvivors.Count) + "\n" +
        string.Format(_loc.T(lang, "ProfileOwnedPerks"), u.OwnedPerks.Count) + "\n" +
        string.Format(_loc.T(lang, "ProfileMissingPerks"), u.MissingPerks.Count);

        await msg.Channel.SendMessageAsync(text);
    }
}
