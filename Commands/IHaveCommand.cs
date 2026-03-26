using Discord.WebSocket;

namespace DBDPerkBot.Commands;

public class IHaveCommand
{
    private readonly UserSettingsService _users;
    private readonly PerkStore _store;
    private readonly LocaleService _loc;

    public IHaveCommand(UserSettingsService users, PerkStore store, LocaleService loc)
    {
        _users = users;
        _store = store;
        _loc = loc;
    }

    public async Task Handle(SocketMessage msg)
    {
        if (!CommandParser.TryMatch(msg.Content, "ihave", out var args)) return;


        var lang = _users.GetLang(msg.Author.Id);

        if (string.IsNullOrWhiteSpace(args))
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "IHave_Usage"));
            return;
        }

        string query = args.Trim().ToLowerInvariant();
        var user = _users.GetSettings(msg.Author.Id);

        // ---------- TRY CHARACTER FIRST ----------
        var survivor = _store.SurvivorPerks
            .Select(p => p.name)
            .Distinct()
            .FirstOrDefault(c => c.ToLower().Contains(query));

        if (survivor != null)
        {
            if (DbDConstants.BaseSurvivors.Contains(survivor))
            {
                await msg.Channel.SendMessageAsync(_loc.T(lang, "CantModifyBaseSurvivor"));
                return;
            }

            var perks = _store.SurvivorPerks
                .Where(p => p.name == survivor)
                .Select(p => p.perk)
                .ToList();

            foreach (var p in perks)
            {
                if (!DbDConstants.BasePerks.Contains(p))
                {
                    user.OwnedPerks.Add(p);
                    user.MissingPerks.Remove(p);
                }
            }

            user.OwnedSurvivors.Add(survivor);
            user.MissingSurvivors.Remove(survivor);

            await _users.SaveAsync(msg.Author.Id);

            await msg.Channel.SendMessageAsync(
                _loc.T(lang, "SurvivorAdded", survivor, perks.Count)
            );
            return;
        }

        // ---------- IF NOT A SURVIVOR → TRY PERK ----------
        var perk = _store.SurvivorPerks
            .FirstOrDefault(p => p.perk.ToLower().Contains(query));

        if (perk == null)
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "Perk_NotFound"));
            return;
        }

        if (DbDConstants.BasePerks.Contains(perk.perk))
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "CantModifyBasePerk"));
            return;
        }

    }
}