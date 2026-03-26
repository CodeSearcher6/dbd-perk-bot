using Discord;
using Discord.WebSocket;
using DBDPerkBot.Commands;


namespace DBDPerkBot;

public class DiscordBot
{
    private readonly DiscordSocketClient _client;
    private readonly BotConfig _config;
    private readonly DpsmCommand _dpsm;
    private readonly ModeCommand _mode;
    private readonly LangCommand _lang;
    private readonly ProfileCommand _profile;
    private readonly CheckIconsCommand _checkIcons;
    private readonly HelpCommand _helpCommand;
    private readonly DhtcCommand _dhtcCommand;
    private readonly DhtpCommand _dhtpCommand;
    private readonly StupidUsersCommand _stupidUsers;
    private readonly IHaveCommand _ihaveCommand;
    private readonly MissingCommand _missingCommand;
    private readonly ResetPerksCommand _resetPerks;
    private readonly IAmNewbieCommand _iAmNewbie;
    private readonly CharacterPerksCommand _characterPerks;




    private readonly UserSettingsService _users;

    public DiscordBot(
        BotConfig config,
        DpsmCommand dpsm,
        ModeCommand mode,
        LangCommand lang,
        ProfileCommand profile,
        CheckIconsCommand checkIcons,
        HelpCommand helpCommand,
        DhtcCommand dhtcCommand,
        DhtpCommand dhtpCommand,
        IHaveCommand ihaveCommand,
        MissingCommand missingCommand,
        ResetPerksCommand resetPerks,
        StupidUsersCommand stupidUsers,
        IAmNewbieCommand iAmNewbie,
        CharacterPerksCommand characterPerks,




        UserSettingsService users
    )
    {
        _dpsm = dpsm;
        _mode = mode;
        _lang = lang;
        _profile = profile;
        _checkIcons = checkIcons;
        _helpCommand = helpCommand;
        _dhtcCommand = dhtcCommand;
        _dhtpCommand = dhtpCommand;
        _ihaveCommand = ihaveCommand;
        _missingCommand = missingCommand;
        _stupidUsers = stupidUsers;
        _resetPerks = resetPerks;
        _iAmNewbie = iAmNewbie;
        _characterPerks = characterPerks;



        _users = users;
        _config = config;



        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        });
    }

    public async Task RunAsync()
    {
        _client.Log += msg =>
        {
            Console.WriteLine($"[Discord] {msg.Message}");
            return Task.CompletedTask;
        };

        _client.MessageReceived += OnMessage;

        await _client.LoginAsync(TokenType.Bot, _config.Token);
        await _client.StartAsync();

        Console.WriteLine("✅ Bot started");
    }

    private async Task OnMessage(SocketMessage msg)
    {
        if (msg.Author.IsBot) return;

        var command = GetCommandName(msg.Content);
        if (command is null) return;

        var handler = ResolveHandler(command);
        if (handler is null) return;

        try
        {
            await handler(msg);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[COMMAND ERROR] {command}: {ex}");
        }
    }

    private static string? GetCommandName(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        content = content.Trim();

        if (content.Length < 2)
            return null;

        var prefix = content[0];
        if (prefix != '!')
            return null;

        var rest = content[1..];
        var spaceIndex = rest.IndexOf(' ');

        return (spaceIndex >= 0 ? rest[..spaceIndex] : rest)
            .Trim()
            .ToLowerInvariant();
    }

    private Func<SocketMessage, Task>? ResolveHandler(string command) => command switch
    {
        "lang" => _lang.Handle,
        "mode" => _mode.Handle,
        "dpsm" => _dpsm.Handle,
        "profile" => _profile.Handle,
        "checkicons" => _checkIcons.Handle,
        "help" => _helpCommand.Handle,
        "dhtc" => _dhtcCommand.Handle,
        "dhtp" => _dhtpCommand.Handle,
        "ihave" => _ihaveCommand.Handle,
        "missing" => _missingCommand.Handle,
        "resetperks" => _resetPerks.Handle,
        "iamnewbie" => _iAmNewbie.Handle,
        "charperks" => _characterPerks.Handle,
        "scp" => _characterPerks.Handle,
        "sex_with_albina" => _stupidUsers.Handle,
        "gaysex_with_vlad" => _stupidUsers.Handle,
        "sex_with_ada" => _stupidUsers.Handle,
        _ => null
    };

}
