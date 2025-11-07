using Discord.WebSocket;

namespace DBDPerkBot.Commands;

public class StupidUsersCommand
{
    private readonly LocaleService _loc;
    private readonly UserSettingsService _users;

    public StupidUsersCommand(LocaleService loc, UserSettingsService users)
    {
        _loc = loc;
        _users = users;
    }

    public async Task Handle(SocketMessage msg)
    {
        // Тригери на будь-яку з фраз
        var triggers = new[] { "!sex_with_Albina", "!gaysex_with_Vlad", "!sex_with_ada","sex_with_Ada", "sex_with_ADA" };
        if (!triggers.Any(t => msg.Content.StartsWith(t, StringComparison.OrdinalIgnoreCase)))
            return;

        var lang = _users.GetLang(msg.Author.Id);
        var angryText = _loc.T(lang, "ReplyAngry"); 

        // Шлях до файлу
        var filePath = Path.Combine(AppContext.BaseDirectory, "assets", "DoorBellAda.png");
        if (!File.Exists(filePath))
            filePath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "DoorBellAda.jpg");

        if (!File.Exists(filePath))
        {
            await msg.Channel.SendMessageAsync(
                _loc.T(lang, "ReplyAngryMissingImage") // локаль для fallback
            );
            return;
        }

        await msg.Channel.SendFileAsync(filePath, angryText);
    }
}
