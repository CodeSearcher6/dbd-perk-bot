using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class CheckMissingIcons
{
    private static string Normalize(string s) =>
        s.Replace("iconperks_", "")
         .Replace("t_ui_iconperks_", "")
         .Replace("iconperks", "")
         .Replace("t_ui_iconperks", "")
         .Replace("-", "")
         .Replace("_", "")
         .Replace(".png", "")
         .Trim()
         .ToLower();

    public static void Run(string assetsPath)
    {
        Console.WriteLine("🔍 Checking perk icons with normalization...");

        string perksFolder = Path.Combine(assetsPath, "Perks");
        string metaFile = Path.Combine(assetsPath, "perks_meta.json");

        if (!Directory.Exists(perksFolder))
        {
            Console.WriteLine($"⚠️ Folder not found: {perksFolder}");
            return;
        }

        if (!File.Exists(metaFile))
        {
            Console.WriteLine($"⚠️ perks_meta.json not found: {metaFile}");
            return;
        }

        var json = File.ReadAllText(metaFile);
        var perks = System.Text.Json.JsonSerializer.Deserialize<List<PerkMeta>>(json);
        if (perks == null || perks.Count == 0)
        {
            Console.WriteLine("❌ No perks inside perks_meta.json");
            return;
        }

        var files = Directory.GetFiles(perksFolder, "*.*", SearchOption.AllDirectories)
            .ToDictionary(
                f => Normalize(Path.GetFileName(f)),
                f => Path.GetFileName(f),
                StringComparer.OrdinalIgnoreCase
            );

        int missing = 0;

        foreach (var perk in perks)
        {
            string normalized = Normalize(perk.image);
            if (!files.ContainsKey(normalized))
            {
                missing++;
                Console.WriteLine($"❌ Missing icon: {perk.image} → `{normalized}` ({perk.perk})");
            }
        }

        if (missing == 0)
            Console.WriteLine("✅ All perk icons exist!");
        else
            Console.WriteLine($"⚠️ Found {missing} missing icons after normalization check.");
    }
}
