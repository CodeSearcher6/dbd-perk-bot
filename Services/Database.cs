using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.IO;

public static class Database
{
    static string DbDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\data"));
    static string DbPath = Path.Combine(DbDir, "database.db");
    private static string DbFile => DbPath;

    public static async Task EnsureInitializedAsync()
    {
        if (!Directory.Exists(DbDir))
            Directory.CreateDirectory(DbDir);

        if (!File.Exists(DbPath))
            await Initialize();
    }

    public static async Task Initialize()
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS missing_perks (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            user_id INTEGER NOT NULL,
            perk_name TEXT NOT NULL,
            UNIQUE(user_id, perk_name)
        );

        CREATE TABLE IF NOT EXISTS user_settings (
            user_id INTEGER PRIMARY KEY,
            mode TEXT DEFAULT 'normal'
        );
        ";

        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task AddMissingAsync(ulong userId, string perkId)
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT OR IGNORE INTO missing_perks (user_id, perk_name) VALUES ($u, $p)";
        cmd.Parameters.AddWithValue("$u", (long)userId);
        cmd.Parameters.AddWithValue("$p", perkId);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<List<string>> GetMissingAsync(ulong userId)
    {
        var list = new List<string>();
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT perk_name FROM missing_perks WHERE user_id = $u";
        cmd.Parameters.AddWithValue("$u", (long)userId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(reader.GetString(0));
        return list;
    }

    public static async Task RemoveMissingAsync(ulong userId, string perkId)
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM missing_perks WHERE user_id = $u AND perk_name = $p";
        cmd.Parameters.AddWithValue("$u", (long)userId);
        cmd.Parameters.AddWithValue("$p", perkId);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task ResetAsync(ulong userId)
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM missing_perks WHERE user_id = $u";
        cmd.Parameters.AddWithValue("$u", (long)userId);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task SetModeAsync(ulong userId, string mode)
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO user_settings (user_id, mode) VALUES ($u, $m)
            ON CONFLICT(user_id) DO UPDATE SET mode = $m;
        ";
        cmd.Parameters.AddWithValue("$u", (long)userId);
        cmd.Parameters.AddWithValue("$m", mode);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<string> GetModeAsync(ulong userId)
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT mode FROM user_settings WHERE user_id = $u";
        cmd.Parameters.AddWithValue("$u", (long)userId);
        var res = await cmd.ExecuteScalarAsync();
        return res == null ? "normal" : (string)res;
    }
}
