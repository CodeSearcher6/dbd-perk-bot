public class UserSettings
{
    public string Mode { get; set; } = "normal";
    public string Language { get; set; } = "en";
    public HashSet<string> OwnedPerks { get; set; } = new();
    public HashSet<string> MissingPerks { get; set; } = new();

    public HashSet<string> OwnedSurvivors { get; set; } = new();
    public HashSet<string> MissingSurvivors { get; set; } = new();

    // NEW — для !dhtp 1 3
    public List<string> LastRolledPerks { get; set; } = new();
}