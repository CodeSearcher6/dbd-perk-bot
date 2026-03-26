using DBDPerkBot.Commands;
using Discord.WebSocket;
using DBDPerkBot;

namespace DBDPerkBot.Commands;

public class DhtcCommand
{
    private readonly UserSettingsService _users;
    private readonly PerkStore _store;
    private readonly LocaleService _loc;


    public DhtcCommand(UserSettingsService users, PerkStore store, LocaleService loc)
    {
        _users = users;
        _loc = loc;
        _store = store;
    }
    public async Task Handle(SocketMessage msg)
    {
        if (!CommandParser.TryMatch(msg.Content, "dhtc", out var args)) return;

        var lang = _users.GetLang(msg.Author.Id);

        if (string.IsNullOrWhiteSpace(args))
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "Dhtc_Usage"));
            return;
        }

        string name = args.Trim();
        var user = _users.GetSettings(msg.Author.Id);

        // базовий сурв — не можна
        if (DbDConstants.BaseSurvivors.Contains(name))
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "Dhtc_constSurv"));
            return;
        }

        var perks = _store.SurvivorPerks
            .Where(p => p.name == name)
            .Select(p => p.perk)
            .ToList();

        if (perks.Count == 0)
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "Dhtc_NotFound"));
            return;
        }

        foreach (var perk in perks)
        {
            if (DbDConstants.BasePerks.Contains(perk)) continue;
            user.MissingPerks.Add(perk);
            user.OwnedPerks.Remove(perk);
        }

        user.MissingSurvivors.Add(name);
        user.OwnedSurvivors.Remove(name);

        await _users.SaveAsync(msg.Author.Id);

        var msgText = _loc.T(lang, "Dhtc_Success")
        .Replace("{name}", name)
        .Replace("{perks.Count}", perks.Count.ToString());

        await msg.Channel.SendMessageAsync(msgText);
    }
}
