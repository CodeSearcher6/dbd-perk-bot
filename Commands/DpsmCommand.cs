using Discord.WebSocket;

namespace DBDPerkBot;

public class DpsmCommand
{
    private readonly BuildGenerator _generator;
    private readonly ImageComposer _image;
    private readonly UserSettingsService _users;
    private readonly LocaleService _loc;

    public DpsmCommand(BuildGenerator generator, ImageComposer image, UserSettingsService users, LocaleService loc)
    {
        _generator = generator;
        _image = image;
        _users = users;
        _loc = loc;
    }

    public async Task Handle(SocketMessage msg)
    {
        if (!msg.Content.StartsWith("!dpsm")) return;

        var userId = msg.Author.Id;
        var lang = _users.GetLang(userId);
        var mode = _users.GetMode(userId);

        await msg.Channel.SendMessageAsync(_loc.T(lang, "Generating"));

        // Генеруємо білд — без Task.Run (це вже async I/O)
        var perks = await _generator.Generate(mode, userId);

        // Зберігаємо дані користувача
        var user = _users.GetSettings(userId);
        user.LastRolledPerks = perks.Select(p => p.Name).ToList();
        await _users.SaveAsync(userId);

        // Рендеримо картинку у фоновому потоці (CPU-bound)
        var stream = await _image.ComposeAsync(perks, mode);
        stream.Position = 0;

        string text = string.Join(", ", perks.Select(p => p.Name));

        await msg.Channel.SendFileAsync(
            stream,
            "build.png",
            $"{_loc.T(lang, "BuildReady")}\n{text}"
        );
    }
}
