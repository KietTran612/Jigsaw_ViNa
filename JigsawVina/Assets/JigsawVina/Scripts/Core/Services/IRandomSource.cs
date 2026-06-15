namespace JigsawVina.Core.Services
{
    public interface IRandomSource
    {
        float NextFloat();
        int NextRange(int minInclusive, int maxExclusive);
    }
}
