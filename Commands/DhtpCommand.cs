using Discord.WebSocket;
using DBDPerkBot;
using DBDPerkBot.Commands;

namespace DBDPerkBot.Commands;

public class DhtpCommand
{
    private readonly UserSettingsService _users;
    private readonly LocaleService _loc;

    public DhtpCommand(UserSettingsService users, LocaleService loc)
    {
        _users = users;
        _loc = loc;
    }

    public async Task Handle(SocketMessage msg)
    {
        if (!msg.Content.StartsWith("!dhtp")) return;

        var lang = _users.GetLang(msg.Author.Id);
        var parts = msg.Content.Split(" ", 2, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "DhtpNothingParsed"));
            return;
        }

        var indexesRaw = parts[1]
            .Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        var parsed = new List<int>();

        foreach (var token in indexesRaw)
        {
            if (int.TryParse(token, out int idx) && idx >= 1 && idx <= 4)
                parsed.Add(idx - 1);
        }

        if (parsed.Count == 0)
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "DhtpNothingParsed"));
            return;
        }

        var user = _users.GetSettings(msg.Author.Id);

        if (user.LastRolledPerks.Count == 0)
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "Dhtp_Warn"));
            return;
        }

        // перевіряємо межі
        if (parsed.Any(i => i < 0 || i >= user.LastRolledPerks.Count))
        {
            var wrong = string.Join(", ", indexesRaw);
            await msg.Channel.SendMessageAsync(
                _loc.T(lang, "DhtpInvalidRange", user.LastRolledPerks.Count, wrong)
            );
            return;
        }

        foreach (var i in parsed)
        {
            var perk = user.LastRolledPerks[i];

            if (DbDConstants.BasePerks.Contains(perk))
                continue; // базові не чіпаємо

            user.MissingPerks.Add(perk);
            user.OwnedPerks.Remove(perk);
        }

        await _users.SaveAsync(msg.Author.Id);

        await msg.Channel.SendMessageAsync(_loc.T(lang, "DhtpSaved"));
    }
}
