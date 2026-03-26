using System.Collections.Concurrent;
using System.Text.Json;

namespace DBDPerkBot;

public class UserSettingsService
{
    private readonly string _file = "user_settings.json";

    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private readonly ConcurrentDictionary<ulong, string> _modes = new();
    private readonly ConcurrentDictionary<ulong, string> _langs = new();
    private readonly ConcurrentDictionary<ulong, UserSettings> _data = new();

    private readonly JsonSerializerOptions _jsonOpts = new() { WriteIndented = true };

    public UserSettingsService()
    {
        LoadAsync().GetAwaiter().GetResult();
    }
    private SettingsModel Snapshot()
    {
        return new SettingsModel
        {
            modes = _modes.ToDictionary(x => x.Key, x => x.Value),
            langs = _langs.ToDictionary(x => x.Key, x => x.Value),
            users = _data.ToDictionary(x => x.Key, x => x.Value)
        };
    }

    // 🔹 Головний метод збереження
    public async Task SaveAsync()
    {
        await _saveLock.WaitAsync();
        try
        {
            var data = Snapshot();
            var tmp = _file + ".tmp";

            await using (var fs = File.Create(tmp))
            {
                await JsonSerializer.SerializeAsync(fs, data, _jsonOpts);
                await fs.FlushAsync();
            }

            File.Move(tmp, _file, true);
        }
        finally
        {
            _saveLock.Release();
        }
    }


    private Task QueueSave()
    {
        return Task.Run(async () =>
        {
            try { await SaveAsync(); }
            catch (Exception ex) { Console.WriteLine($"[SAVE] {ex.Message}"); }
        });
    }

    // 🔹 Обгортка для сумісності зі старими викликами
    public Task SaveAsync(ulong userId) => SaveAsync();

    public UserSettings GetSettings(ulong userId)
    {
        if (!_data.TryGetValue(userId, out var st))
        {
            st = new UserSettings();
            _data[userId] = st;
            _ = QueueSave(); // фонове збереження
        }
        return st;
    }

    public string GetMode(ulong id)
    {
        if (!_modes.TryGetValue(id, out var mode))
            return "random";

        return mode.Equals("normal", StringComparison.OrdinalIgnoreCase)
            ? "random"
            : mode;
    }

    public void SetMode(ulong id, string mode)
    {
        _modes[id] = mode;
        _ = QueueSave();
    }

    public string GetLang(ulong id) => _langs.TryGetValue(id, out var l) ? l : "en";
    public void SetLang(ulong id, string lang)
    {
        _langs[id] = lang;
        _ = QueueSave();
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
