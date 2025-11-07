using Microsoft.Extensions.Configuration;

namespace DBDPerkBot;
public class BotConfig
{
    public string Token { get; }
    public string Language { get; }
    public string Mode { get; }
    public ulong LogChannelId { get; }

    public BotConfig(IConfiguration config)
    {
        Token = config["Discord:Token"]
            ?? throw new Exception("Missing Discord token in appsettings.json");

        Language = config["Bot:Language"] ?? "en";
        Mode = config["Bot:Mode"] ?? "normal";
        LogChannelId = ulong.Parse(config["Discord:LogChannelId"] ?? "0");
    }
}
