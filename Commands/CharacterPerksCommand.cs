using Discord.WebSocket;

namespace DBDPerkBot.Commands;

public class CharacterPerksCommand
{
    private readonly PerkStore _store;
    private readonly ImageComposer _image;
    private readonly UserSettingsService _users;
    private readonly LocaleService _loc;

    public CharacterPerksCommand(
        PerkStore store,
        ImageComposer image,
        UserSettingsService users,
        LocaleService loc)
    {
        _store = store;
        _image = image;
        _users = users;
        _loc = loc;
    }

    public async Task Handle(SocketMessage msg)
    {
        string args;

        if (!CommandParser.TryMatch(msg.Content, "charperks", out args) &&
            !CommandParser.TryMatch(msg.Content, "scp", out args))
            return;

        var userId = msg.Author.Id;
        var lang = _users.GetLang(userId);

        if (string.IsNullOrWhiteSpace(args))
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "CharacterPerksUsage"));
            return;
        }

        string query = Normalize(args);

        var character = _store.SurvivorPerks
            .Select(p => p.name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(name => Normalize(name) == query)
            ?? _store.SurvivorPerks
                .Select(p => p.name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(name => Normalize(name).Contains(query));

        if (character == null)
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "CharacterPerksNotFound"));
            return;
        }

        var perks = _store.SurvivorPerks
            .Where(p => string.Equals(p.name, character, StringComparison.OrdinalIgnoreCase))
            .Select(p => new Perk
            {
                Name = p.perk,
                IconPath = p.image
            })
            .DistinctBy(p => p.Name)
            .Take(3)
            .ToList();

        if (perks.Count == 0)
        {
            await msg.Channel.SendMessageAsync(_loc.T(lang, "CharacterPerksNotFound"));
            return;
        }

        string displayName = ToDisplayName(character);
        var stream = await _image.ComposeCharacterPerksAsync(displayName, perks);
        stream.Position = 0;

        string perkList = string.Join(", ", perks.Select(p => p.Name));

        await msg.Channel.SendFileAsync(
            stream,
            "character-perks.png",
            $"{_loc.T(lang, "CharacterPerksReady", displayName)}\n{perkList}"
        );
    }

    private static string Normalize(string value)
    {
        return value.Replace("_", " ").Trim().ToLowerInvariant();
    }

    private static string ToDisplayName(string value)
    {
        return value.Replace("_", " ").Trim();
    }
}
