using System.Collections.Concurrent;
using System.Text.Json;

namespace DBDPerkBot;

public class UserSettingsService
{
    private readonly string _file = "user_settings.json";

    private readonly ConcurrentDictionary<ulong, string> _modes = new();
    private readonly ConcurrentDictionary<ulong, string> _langs = new();
    private readonly ConcurrentDictionary<ulong, UserSettings> _data = new();

    private readonly JsonSerializerOptions _jsonOpts = new() { WriteIndented = true };

    public UserSettingsService()
    {
        LoadAsync().GetAwaiter().GetResult();
    }

    // 🔹 Головний метод збереження
    public Task SaveAsync()
    {
        return Task.Run(async () =>
        {
            var data = new SettingsModel
            {
                modes = _modes.ToDictionary(x => x.Key, x => x.Value),
                langs = _langs.ToDictionary(x => x.Key, x => x.Value),
                users = _data.ToDictionary(x => x.Key, x => x.Value)
            };

            await using var fs = File.Create(_file);
            await JsonSerializer.SerializeAsync(fs, data, _jsonOpts);
        });
    }

    // 🔹 Обгортка для сумісності зі старими викликами
    public Task SaveAsync(ulong userId)
    {
        return SaveAsync(); // ігноруємо userId, бо зберігаємо весь об’єкт
    }

    public UserSettings GetSettings(ulong userId)
    {
        if (!_data.TryGetValue(userId, out var st))
        {
            st = new UserSettings();
            _data[userId] = st;
            _ = SaveAsync(); // фонове збереження
        }
        return st;
    }

    public string GetMode(ulong id) => _modes.TryGetValue(id, out var m) ? m : "normal";
    public void SetMode(ulong id, string m)
    {
        _modes[id] = m;
        _ = SaveAsync();
    }

    public string GetLang(ulong id) => _langs.TryGetValue(id, out var l) ? l : "en";
    public void SetLang(ulong id, string lang)
    {
        _langs[id] = lang;
        _ = SaveAsync();
    }

    private async Task LoadAsync()
    {
        if (!File.Exists(_file)) return;

        await using var fs = File.OpenRead(_file);
        var data = await JsonSerializer.DeserializeAsync<SettingsModel>(fs);

        if (data?.modes != null)
            foreach (var kv in data.modes)
                _modes[kv.Key] = kv.Value;

        if (data?.langs != null)
            foreach (var kv in data.langs)
                _langs[kv.Key] = kv.Value;

        if (data?.users != null)
            foreach (var kv in data.users)
                _data[kv.Key] = kv.Value;
    }

    private class SettingsModel
    {
        public Dictionary<ulong, string>? modes { get; set; }
        public Dictionary<ulong, string>? langs { get; set; }
        public Dictionary<ulong, UserSettings>? users { get; set; }
    }
}
