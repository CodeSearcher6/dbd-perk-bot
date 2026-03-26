using HtmlAgilityPack;

public static class OtzParser
{
    private static readonly HttpClient http = new HttpClient();
    private static readonly SemaphoreSlim _cacheLock = new(1, 1);
    private static List<OtzBuild>? _cachedBuilds;
    private static DateTime _cacheExpiresAtUtc = DateTime.MinValue;
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(10);

    public static async Task<List<OtzBuild>> LoadBuilds(string sectionName)
    {
        string url = "https://mrtipson.github.io/otz-builds/";
        string html = await http.GetStringAsync(url);

        return await Task.Run(() =>
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var sectionNode = doc.DocumentNode.SelectSingleNode(
                $"//h3[contains(@class,'characterName') and contains(text(),'{sectionName}')]"
            );

            if (sectionNode == null)
                throw new Exception($"OTZ section not found: {sectionName}");

            var buildsContainer = sectionNode.ParentNode.SelectSingleNode(".//div[@class='builds']");
            if (buildsContainer == null)
                throw new Exception($"No builds found under section: {sectionName}");

            var builds = new List<OtzBuild>();

            foreach (var buildNode in buildsContainer.SelectNodes(".//div[@class='build']"))
            {
                var build = new OtzBuild { Section = sectionName };

                build.BuildName = buildNode.SelectSingleNode(".//div[@class='buildName']")?.InnerText?.Trim()
                    ?? "Unnamed Build";

                var perkNodes = buildNode.SelectNodes(".//div[@class='perks']//img[contains(@class,'perk')]");
                foreach (var img in perkNodes)
                {
                    string perkName = img.GetAttributeValue("alt", "").Trim();
                    if (!string.IsNullOrEmpty(perkName))
                        build.Perks.Add(perkName);
                }

                var altBlock = buildNode.SelectSingleNode(".//div[@class='alternatives']");
                if (altBlock != null)
                {
                    foreach (var altImg in altBlock.SelectNodes(".//img[contains(@class,'perk')]"))
                    {
                        string altName = altImg.GetAttributeValue("alt", "").Trim();
                        string role = altImg.GetAttributeValue("data-role", "any");

                        if (!string.IsNullOrEmpty(altName))
                        {
                            if (!build.AltPerks.ContainsKey(role))
                                build.AltPerks[role] = new();

                            build.AltPerks[role].Add(altName);
                        }
                    }
                }

                builds.Add(build);
            }

            return builds;
        });
    }

    public static async Task<List<OtzBuild>> LoadAllBuilds()
    {
        var now = DateTime.UtcNow;
        if (_cachedBuilds != null && now < _cacheExpiresAtUtc)
            return _cachedBuilds.ToList();

        await _cacheLock.WaitAsync();
        try
        {
            now = DateTime.UtcNow;
            if (_cachedBuilds != null && now < _cacheExpiresAtUtc)
                return _cachedBuilds.ToList();

            var sections = new[]
            {
            "Builds for Teams",
            "Advanced Builds",
            "Solo Survivors"
        };

            var all = new List<OtzBuild>();

            foreach (var sec in sections)
            {
                try
                {
                    var builds = await LoadBuilds(sec);
                    all.AddRange(builds);
                    Console.WriteLine($"[OTZ] Loaded {builds.Count} builds from {sec}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OTZ] Failed {sec}: {ex.Message}");
                }
            }

            _cachedBuilds = all;
            _cacheExpiresAtUtc = DateTime.UtcNow.Add(CacheLifetime);

            return all.ToList();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public class OtzBuild
    {
        public string Section { get; set; }
        public string BuildName { get; set; }

        public List<string> Perks { get; set; } = new();
        public Dictionary<string, List<string>> AltPerks { get; set; } = new();
    }
}
