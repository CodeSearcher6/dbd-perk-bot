using System.Text.Json;

namespace DBDPerkBot;

public class LocaleService
{
    private readonly Dictionary<string, Dictionary<string, string>> _strings = new();
    private readonly string _default = "en";

    public LocaleService()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "assets", "Locales");

        foreach (var file in Directory.GetFiles(path, "*.json"))
        {
            var lang = Path.GetFileNameWithoutExtension(file).ToLower();
            var json = JsonSerializer.Deserialize<LocaleJson>(File.ReadAllText(file));

            if (json?.Strings != null)
            {
                _strings[lang] = json.Strings.ToDictionary(
                    kv => kv.Key.ToLower(),   // ✅ ключ → lower case
                    kv => kv.Value
                );
            }
        }

        Console.WriteLine($"[LOCALE] Loaded languages: {string.Join(", ", _strings.Keys)}");
        foreach (var (lang, map) in _strings)
        {
            Console.WriteLine($"[LOCALE:{lang}] Loaded {map.Count} strings");
            foreach (var key in map.Keys)
                Console.WriteLine($"  - {key}");
        }
    }

    public string T(string lang, string key, params object[] args)
    {
        key = key.ToLower();

        if (_strings.TryGetValue(lang, out var dict) && dict.TryGetValue(key, out var val))
            return args.Length > 0 ? string.Format(val, args) : val;

        // fallback english
        if (_strings.TryGetValue(_default, out var def) && def.TryGetValue(key, out var defVal))
            return args.Length > 0 ? string.Format(defVal, args) : defVal;

        return key;
    }

    private class LocaleJson
    {
        public Dictionary<string, string> Strings { get; set; } = new();
        public Dictionary<string, string> Perks { get; set; } = new();
    }
}
