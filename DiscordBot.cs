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

    private Task OnMessage(SocketMessage msg)
    {
        if (msg.Author.IsBot) return Task.CompletedTask;

        var handlers = new Func<SocketMessage, Task>[]
        {
        _lang.Handle, _mode.Handle, _dpsm.Handle, _profile.Handle,
        _checkIcons.Handle, _helpCommand.Handle, _dhtcCommand.Handle,
        _dhtpCommand.Handle, _ihaveCommand.Handle, _missingCommand.Handle,
        _resetPerks.Handle, _stupidUsers.Handle
        };

        foreach (var h in handlers)
            _ = Task.Run(() => h(msg));

        return Task.CompletedTask;
    }

}
