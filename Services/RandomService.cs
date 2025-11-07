namespace DBDPerkBot;

public class RandomService
{
    private readonly Random _rng = new();

    public int Next(int max) => _rng.Next(max);
    public double NextDouble() => _rng.NextDouble();
}
