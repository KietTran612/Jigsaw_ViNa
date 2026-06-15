namespace JigsawVina.Core.Services
{
    public class UnityRandomSource : IRandomSource
    {
        public float NextFloat()
        {
            return UnityEngine.Random.value;
        }

        public int NextRange(int minInclusive, int maxExclusive)
        {
            return UnityEngine.Random.Range(minInclusive, maxExclusive);
        }
    }
}
