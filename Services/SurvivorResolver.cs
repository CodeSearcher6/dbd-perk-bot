using DBDPerkBot;
public class SurvivorResolver
{
    private readonly List<string> _names;

    public SurvivorResolver(PerkStore store)
    {
        _names = store.SurvivorPerks
            .Select(p => p.name)
            .Distinct()
            .ToList();
    }

    public string? Resolve(string input)
    {
        input = input.Trim().ToLower();
        return _names.FirstOrDefault(n => n.ToLower().StartsWith(input));
    }
}
