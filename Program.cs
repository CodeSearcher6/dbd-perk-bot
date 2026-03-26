using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DBDPerkBot;
using DBDPerkBot.Commands;
using DBDPerkBot.Services;


var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<BotConfig>();
        services.AddSingleton<DiscordBot>();

        services.AddSingleton<PerkStore>();
        services.AddSingleton<RandomService>();
        services.AddSingleton<BuildGenerator>();
        services.AddSingleton<ImageComposer>();

        services.AddSingleton<DpsmCommand>();
        services.AddSingleton<ModeCommand>();
        services.AddSingleton<LangCommand>();
        services.AddSingleton<ProfileCommand>();
        services.AddSingleton<CheckIconsCommand>();
        services.AddSingleton<HelpCommand>();
        services.AddSingleton<DhtcCommand>();
        services.AddSingleton<IHaveCommand>();
        services.AddSingleton<MissingCommand>();
        services.AddSingleton<DhtpCommand>();
        services.AddSingleton<ResetPerksCommand>();
        services.AddSingleton<StupidUsersCommand>();
        services.AddSingleton<IAmNewbieCommand>();
        services.AddSingleton<CharacterPerksCommand>();




        services.AddSingleton<UserSettingsService>();
        services.AddSingleton<LocaleService>();


    });

IHost app = builder.Build();

await Database.EnsureInitializedAsync();

var bot = app.Services.GetRequiredService<DiscordBot>();
await bot.RunAsync();

await Task.Delay(-1);
