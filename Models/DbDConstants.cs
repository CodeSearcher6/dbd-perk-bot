using DBDPerkBot.Models;
public static class DbDConstants
{
    public static readonly HashSet<string> BaseSurvivors = new()
    {
        "Dwight Fairfield", "Meg Thomas", "Claudette Morel", "Jake Park"
    };

    public static readonly HashSet<string> BasePerks = new()
    {
        // Dwight
        "Bond", "Prove Thyself", "Leader",
        // Meg
        "Quick & Quiet", "Sprint Burst", "Adrenaline",
        // Claudette
        "Empathy", "Botany Knowledge", "Self-Care",
        // Jake
        "Iron Will", "Calm Spirit", "Saboteur"
    };
}
