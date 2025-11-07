using Discord.WebSocket;
using DBDPerkBot.Services;
using System.IO;
using System.Linq;

namespace DBDPerkBot.Commands;

public class CheckIconsCommand
{
    private readonly PerkStore _store;
    private readonly string _assetsPath;

    public CheckIconsCommand(PerkStore store)
    {
        _store = store;
        _assetsPath = Path.Combine(AppContext.BaseDirectory, "assets", "Perks");
    }

    public async Task Handle(SocketMessage msg)
    {
        if (!msg.Content.StartsWith("!checkicons")) return;

        try
        {
            if (!Directory.Exists(_assetsPath))
            {
                await msg.Channel.SendMessageAsync($"❌ Assets folder not found: `{_assetsPath}`");
                return;
            }

            var files = Directory.GetFiles(_assetsPath)
                .Select(f => Normalize(Path.GetFileName(f)))
                .ToHashSet();

            var missing = new List<string>();

            foreach (var perk in _store.SurvivorPerks)
            {
                var name = Normalize(perk.image);
                if (!files.Contains(name))
                    missing.Add(perk.image);
            }

            if (missing.Count == 0)
            {
                await msg.Channel.SendMessageAsync("✅ Усі іконки знайдені!");
                return;
            }

            // якщо мало — виводимо в чат
            if (missing.Count <= 20)
            {
                string report = "⚠️ Відсутні іконки:\n```\n" + string.Join("\n", missing) + "\n```";
                await msg.Channel.SendMessageAsync(report);
                return;
            }

            // якщо багато — зберігаємо у файл і шлемо
            var filePath = Path.Combine(Path.GetTempPath(), "missing_icons.txt");
            await File.WriteAllLinesAsync(filePath, missing);

            await msg.Channel.SendFileAsync(filePath, "📦 Багато відсутніх іконок — дивись файл");

            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR][CheckIcons] {ex}");
            await msg.Channel.SendMessageAsync("❌ Error in !checkicons — check console.");
        }
    }

    private string Normalize(string s) =>
    s.ToLower()
     .Replace("iconperks", "")
     .Replace("iconsperks", "")
     .Replace("iconperk", "")
     .Replace("iconsperk", "")
     .Replace("t_ui_iconperks", "")
     .Replace("t_iconperks", "")
     .Replace("t_ui_iconsperks", "")
     .Replace("t_iconsperks", "")
     .Replace("_", "")
     .Replace("-", "")
     .Replace(" ", "")
     .Replace(".png", "")
     .Replace(".jpg", "")
     .Replace(".jpeg", "")
     .Trim();

}