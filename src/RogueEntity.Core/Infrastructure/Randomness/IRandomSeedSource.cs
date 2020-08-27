namespace RogueEntity.Core.Infrastructure.Randomness
{
    public interface IRandomSeedSource
    {
        public int AsRandomSeed();
    }
}