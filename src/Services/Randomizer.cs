namespace Services;

public sealed class Randomizer
{
    private readonly Random _random;

    public Randomizer(Random random)
    {
        _random = random;
    }

    public (T[], T[]) RandomDistribute<T>(T[] src, double probability)
    {
        if (probability is < 0 or > 1)
        {
            throw new ArgumentException("Probability must be between 0 and 1.");
        }

        var length = src.Length;
        var length1 = (int) (length * probability);
        var length2 = length - length1;

        var list1 = new List<T>(length1);
        var list2 = new List<T>(length2);

        foreach (var e in src)
        {
            if (_random.NextDouble() < length1 / (double) length)
            {
                list1.Add(e);
                length1--;
            }
            else
            {
                list2.Add(e);
            }

            length--;
        }

        return (list1.ToArray(), list2.ToArray());
    }
}