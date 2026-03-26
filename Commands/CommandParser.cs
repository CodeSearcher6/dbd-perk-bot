namespace DBDPerkBot.Utils;

public static class CommandParser
{
    public static bool TryMatch(string? content, string command, out string args)
    {
        args = string.Empty;

        if (string.IsNullOrWhiteSpace(content))
            return false;

        content = content.Trim();

        if (content.Length < 2)
            return false;

        var prefix = content[0];
        if (prefix != '!' && prefix != '/')
            return false;

        var expected = prefix + command;

        if (!content.StartsWith(expected, StringComparison.OrdinalIgnoreCase))
            return false;

        if (content.Length > expected.Length && !char.IsWhiteSpace(content[expected.Length]))
            return false;

        args = content.Length > expected.Length
            ? content[expected.Length..].Trim()
            : string.Empty;

        return true;
    }
}
