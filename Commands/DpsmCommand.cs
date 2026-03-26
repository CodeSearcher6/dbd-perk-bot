using Discord.WebSocket;
using System.Collections.Concurrent;


namespace DBDPerkBot;

public class DpsmCommand
{
    private readonly BuildGenerator _generator;
    private readonly ImageComposer _image;
    private readonly UserSettingsService _users;
    private readonly LocaleService _loc;
    private static readonly SemaphoreSlim _buildGate = new(3, 3);
    private static readonly ConcurrentDictionary<ulong, SemaphoreSlim> _userLocks = new();


    public DpsmCommand(BuildGenerator generator, ImageComposer image, UserSettingsService users, LocaleService loc)
    {
        _generator = generator;
        _image = image;
        _users = users;
        _loc = loc;
    }

    public async Task Handle(SocketMessage msg)
    {
        if (!CommandParser.TryMatch(msg.Content, "dpsm", out _)) return;

        var userId = msg.Author.Id;
        var userLock = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
        var lang = _users.GetLang(userId);

        await userLock.WaitAsync();
        await _buildGate.WaitAsync();
        try
        {

            var mode = _users.GetMode(userId);

            await msg.Channel.SendMessageAsync(_loc.T(lang, "Generating"));

            var perks = await _generator.Generate(mode, userId);

            var user = _users.GetSettings(userId);
            user.LastRolledPerks = perks.Select(p => p.Name).ToList();
            await _users.SaveAsync(userId);

            var stream = await _image.ComposeAsync(perks, mode);
            stream.Position = 0;

            string text = string.Join(", ", perks.Select(p => p.Name));

            await msg.Channel.SendFileAsync(
                stream,
                "build.png",
                $"{_loc.T(lang, "BuildReady")}\n{text}"
            );
        }
        finally
        {
            _buildGate.Release();
            userLock.Release();

            if (userLock.CurrentCount == 1)
                _userLocks.TryRemove(userId, out _);
        }
    }

}
