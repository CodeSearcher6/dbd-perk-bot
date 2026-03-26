namespace DBDPerkBot;

public class RandomService
{
    public int Next(int max) => Random.Shared.Next(max);
    public double NextDouble() => Random.Shared.NextDouble();
}
