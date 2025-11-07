using System.Text.Json;
using DBDPerkBot.Models;

namespace DBDPerkBot.Services;

public class PerkStore
{
    public List<PerkMeta> SurvivorPerks { get; private set; } = new();
    private readonly string _assetsPerksPath;
    private readonly Dictionary<string, string> _iconIndex = new(StringComparer.OrdinalIgnoreCase);

    public PerkStore()
    {
        // JSON
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "assets", "perks_meta.json");
        var json = File.ReadAllText(jsonPath);
        SurvivorPerks = JsonSerializer.Deserialize<List<PerkMeta>>(json) ?? new();

        // База іконок
        _assetsPerksPath = Path.Combine(AppContext.BaseDirectory, "assets", "Perks");
        if (!Directory.Exists(_assetsPerksPath))
        {
            Console.WriteLine($"[ERROR] Assets folder not found: {_assetsPerksPath}");
            return;
        }

        // Індекс усіх файлів (включно з підпапками)
        foreach (var file in Directory.GetFiles(_assetsPerksPath, "*.*", SearchOption.AllDirectories))
        {
            var key = Normalize(Path.GetFileName(file));
            if (!_iconIndex.ContainsKey(key))
                _iconIndex[key] = file;
        }
        Console.WriteLine($"[INFO] PerkStore indexed {_iconIndex.Count} icons.");
    }

    /// <summary>Повертає повний шлях до іконки, ігноруючи регістр/префікси/дефіси тощо.</summary>
    public string? ResolveIcon(string iconFromJson)
    {
        var key = Normalize(iconFromJson);
        if (_iconIndex.TryGetValue(key, out var path))
            return path;

        // Трапляються дивні символи/акценти
        key = Normalize(RemoveAccents(iconFromJson));
        if (_iconIndex.TryGetValue(key, out path))
            return path;

        return null;
    }

    public static string Normalize(string s)
    {
        s = s.Trim().ToLowerInvariant();

        // прибрати відомі префікси та шум
        s = s
            .Replace("iconperks", "")
            .Replace("iconsperks", "")
            .Replace("iconperk", "")
            .Replace("iconsperk", "")
            .Replace("t_ui_iconperks", "")
            .Replace("t_iconperks", "")
            .Replace("t_ui_iconsperks", "")
            .Replace("t_iconsperks", "")
            .Replace(".png", "")
            .Replace(".jpg", "")
            .Replace(".jpeg", "")
            .Replace("-", "")
            .Replace("_", "")
            .Replace(" ", "");

        return s;
    }

    public static string RemoveAccents(string s)
    {
        // дуже проста заміна, без важкої нормалізації
        return s
            .Replace("é", "e").Replace("É", "E")
            .Replace("á", "a").Replace("Á", "A")
            .Replace("í", "i").Replace("Í", "I")
            .Replace("ó", "o").Replace("Ó", "O")
            .Replace("ú", "u").Replace("Ú", "U");
    }
}
