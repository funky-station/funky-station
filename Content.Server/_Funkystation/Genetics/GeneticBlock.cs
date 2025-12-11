namespace Content.Server._Funkystation.Genetics;

public readonly record struct GeneticBlock(int Block, string Sequence)
{
    public static readonly GeneticBlock Invalid = new(-1, string.Empty);
}
