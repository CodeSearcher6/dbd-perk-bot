using DBDPerkBot;
public class BuildGenerator
{
    private readonly PerkStore _perkStore;
    private readonly RandomService _rng;
    private readonly UserSettingsService _users;
    private const string LocalBuildsPath = "assets/build_sets.json";


    public BuildGenerator(PerkStore perkStore, RandomService rng, UserSettingsService users)
    {
        _perkStore = perkStore;
        _rng = rng;
        _users = users;
    }

    private List<Perk> FilterOwned(ulong userId, List<Perk> perks)
    {
        var u = _users.GetSettings(userId);

        var list = perks;

        // виключаємо те, чого немає
        if (u.MissingPerks.Count > 0)
            list = list.Where(p => !u.MissingPerks.Contains(p.Name)).ToList();

        // якщо юзер ще не вніс свої перки — даємо весь пул
        if (u.OwnedPerks.Count == 0)
            return list;

        // беремо лише те, що юзер має
        return list
            .Where(p => u.OwnedPerks.Contains(p.Name))
            .ToList();
    }


    private List<Perk> CompleteToFour(List<Perk> pool, List<Perk> fallback, ulong userId)
    {
        var ownedFallback = FilterOwned(userId, fallback);

        var res = pool
            .GroupBy(p => p.Name)
            .Select(g => g.First())
            .Take(4)
            .ToList();

        // Спочатку добиваємо з перків, що юзер має
        while (res.Count < 4 && ownedFallback.Count > 0)
        {
            var add = ownedFallback[_rng.Next(ownedFallback.Count)];
            if (!res.Any(x => x.Name == add.Name))
                res.Add(add);
        }

        // Якщо все ще не вистачає — тоді вже з повного пулу
        while (res.Count < 4 && fallback.Count > 0)
        {
            var add = fallback[_rng.Next(fallback.Count)];
            if (!res.Any(x => x.Name == add.Name))
                res.Add(add);
        }

        return res;
    }


    public Task<List<Perk>> GenerateRandom(ulong userId)
    {
        var all = _perkStore.SurvivorPerks
            .Select(p => new Perk { Name = p.perk, IconPath = p.image })
            .ToList();

        var owned = FilterOwned(userId, all);

        // якщо нема достатньо — добиваємо з повного пулу
        var chosen = CompleteToFour(
        owned.OrderBy(_ => _rng.NextDouble()).ToList(),
        all,
        userId
    );


        return Task.FromResult(chosen);
    }

    public async Task<List<Perk>> Generate(string mode, ulong userId)
    {
        var modeLower = mode.ToLower();

        // 1️⃣ локальні режими
        if (modeLower is "meta" or "troll" or "random")
        {
            var local = await LoadLocalBuild(modeLower);
            if (local.Count > 0)
                return CompleteToFour(local, local, userId);
        }


        if (modeLower is "streamer" or "solo" or "forteams" or "advanced")
        {
            try
            {
                var builds = await OtzParser.LoadAllBuilds();

                if (modeLower == "solo")
                    builds = builds.Where(b => b.Section.ToLower().Contains("solo")).ToList();

                if (modeLower == "forteams")
                    builds = builds.Where(b => b.Section.ToLower().Contains("team")).ToList();

                if (modeLower == "advanced")
                    builds = builds.Where(b => b.Section.ToLower().Contains("advanced")).ToList();

                // ✅ STREAMER MODE — random from Solo / Teams / Advanced
                if (modeLower == "streamer")
                {
                    var rnd = _rng.Next(3);
                    if (rnd == 0) builds = builds.Where(b => b.Section.ToLower().Contains("solo")).ToList();
                    else if (rnd == 1) builds = builds.Where(b => b.Section.ToLower().Contains("team")).ToList();
                    else builds = builds.Where(b => b.Section.ToLower().Contains("advanced")).ToList();
                }

                if (builds.Count == 0)
                    return await GenerateRandom(userId);

                var build = builds[_rng.Next(builds.Count)];

                var raw = build.Perks;
                var all = _perkStore.SurvivorPerks
                    .Select(p => new Perk { Name = p.perk, IconPath = p.image })
                    .ToList();

                var filtered = all
                    .Where(p => raw.Contains(p.Name))
                    .ToList();

                var ownedFiltered = FilterOwned(userId, filtered);

                return CompleteToFour(
                    ownedFiltered,
                    all,
                    userId
                );
            }
            catch
            {
                return await GenerateRandom(userId);
            }
        }

        return await GenerateRandom(userId);
    }
    private async Task<List<Perk>> LoadLocalBuild(string mode)
    {
        var path = Path.Combine(AppContext.BaseDirectory, LocalBuildsPath);
        if (!File.Exists(path))
            return new();

        try
        {
            using var fs = File.OpenRead(path);
            var json = await System.Text.Json.JsonSerializer.DeserializeAsync<Dictionary<string, LocalBuild>>(fs);
            if (json == null || !json.TryGetValue(mode.ToLower(), out var data))
                return new();

            // Якщо там import_otz = true → ми віддамо пустий список, щоб перейти в OtzParser
            if (data.import_otz)
                return new();

            // Якщо random = true → згенеруємо просто випадкові
            if (data.random)
                return await GenerateRandom(0);

            // Якщо задано perks → зберемо їх
            if (data.perks != null && data.perks.Count > 0)
            {
                var all = _perkStore.SurvivorPerks
                    .Select(p => new Perk { Name = p.perk, IconPath = p.image })
                    .ToList();

                return all
                    .Where(p => data.perks.Contains(p.Name))
                    .OrderBy(_ => _rng.NextDouble()) 
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOCAL BUILDS] Error reading file: {ex.Message}");
        }

        return new();
    }
    private class LocalBuild
    {
        public List<string>? perks { get; set; }
        public bool import_otz { get; set; }
        public bool random { get; set; }
    }
}
