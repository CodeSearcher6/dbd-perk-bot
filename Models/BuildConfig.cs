using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class BuildSet
{
    public List<string>? perks { get; set; }
    public List<string>? local_perks { get; set; }
    public bool random { get; set; }
    public bool import_otz { get; set; }
}

public static class BuildConfig
{
    public static Dictionary<string, BuildSet> Sets { get; private set; } = new();

    public static void Load(string assetsPath)
    {
        var path = Path.Combine(assetsPath, "build_sets.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("build_sets.json not found at " + path);

        var json = File.ReadAllText(path);
        Sets = JsonSerializer.Deserialize<Dictionary<string, BuildSet>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new Dictionary<string, BuildSet>();
    }

    public static BuildSet Get(string mode)
    {
        // дефолт — normal/random
        if (string.IsNullOrWhiteSpace(mode)) mode = "normal";
        if (!Sets.TryGetValue(mode, out var set))
            Sets.TryGetValue("normal", out set);
        return set ?? new BuildSet { random = true };
    }
}
