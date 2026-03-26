using Discord.WebSocket;

namespace DBDPerkBot.Commands;

public class IAmNewbieCommand
{
    private readonly UserSettingsService _users;
    private readonly PerkStore _store;
    private readonly LocaleService _loc;

    public IAmNewbieCommand(UserSettingsService users, PerkStore store, LocaleService loc)
    {
        _users = users;
        _store = store;
        _loc = loc;
    }

    public async Task Handle(SocketMessage msg)
    {
        if (!CommandParser.TryMatch(msg.Content, "iamnewbie", out _)) return;

        var userId = msg.Author.Id;
        var lang = _users.GetLang(userId);
        var user = _users.GetSettings(userId);

        var allSurvivors = _store.SurvivorPerks
            .Select(p => p.name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var allEntries = _store.SurvivorPerks
            .Where(p => !string.IsNullOrWhiteSpace(p.perk))
            .ToList();

        var allPerks = allEntries
            .Select(p => p.perk)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var generalPerks = allEntries
            .Where(IsGeneralEntry)
            .Select(p => p.perk)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);


        var baseSurvivors = DbDConstants.BaseSurvivors
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var basePerks = DbDConstants.BasePerks
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        user.OwnedSurvivors = allSurvivors
            .Where(s => baseSurvivors.Contains(s))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        user.MissingSurvivors = allSurvivors
            .Where(s => !baseSurvivors.Contains(s))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        user.OwnedPerks = allPerks
            .Where(p => basePerks.Contains(p) || generalPerks.Contains(p))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        user.MissingPerks = allPerks
            .Where(p => !basePerks.Contains(p))
            .Where(p => !generalPerks.Contains(p))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);


        user.LastRolledPerks.Clear();

        await _users.SaveAsync(userId);

        await msg.Channel.SendMessageAsync(
            _loc.T(
                lang,
                "IAmNewbieDone",
                user.MissingSurvivors.Count,
                user.MissingPerks.Count
            )
        );
    }

    private static bool IsGeneralEntry(PerkMeta meta)
    {
        if (meta == null)
            return false;

        var role = meta.role?.Trim().ToLowerInvariant() ?? string.Empty;
        var ownerName = meta.name?.Trim().ToLowerInvariant() ?? string.Empty;

        return role.Contains("general") || ownerName.Contains("general");
    }
}
