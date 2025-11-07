using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace DBDPerkBot.Commands;

public class HelpCommand
{
    private readonly LocaleService _loc;
    private readonly UserSettingsService _users;

    public HelpCommand(LocaleService loc, UserSettingsService users)
    {
        _loc = loc;
        _users = users;
    }

    public async Task Handle(SocketMessage msg)
    {
        if (!msg.Content.StartsWith("!help")) return;

        var lang = _users.GetLang(msg.Author.Id);

        var embed = new EmbedBuilder()
            .WithTitle(_loc.T(lang, "HelpTitle"))
            .WithDescription(_loc.T(lang, "HelpDesc"))
            .WithColor(new Color(139, 0, 255)) // purple
            .AddField(_loc.T(lang, "HelpDpsm"), "\u200B")
            .AddField(_loc.T(lang, "HelpLang"), "\u200B")
            .AddField(_loc.T(lang, "HelpDhtp"), "\u200B")
            .AddField(_loc.T(lang,"HelpDhtc"), "\u200B")
            .AddField(_loc.T(lang, "HelpIHave"), "\u200B")
            .AddField(_loc.T(lang, "HelpMissing"), "\u200B")
            .AddField(_loc.T(lang, "HelpReset"), "\u200B")
            .AddField(_loc.T(lang, "HelpMode"), "\u200B")
            .AddField(_loc.T(lang, "HelpProfile"), "\u200B")
            .AddField(_loc.T(lang, "HelpHelp"), "\u200B");

        await msg.Channel.SendMessageAsync(embed: embed.Build());
    }
}
