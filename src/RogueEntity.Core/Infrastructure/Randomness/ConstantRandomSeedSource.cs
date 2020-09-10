namespace RogueEntity.Core.Infrastructure.Randomness
{
    public readonly struct ConstantRandomSeedSource: IRandomSeedSource
    {
        readonly int value;

        public ConstantRandomSeedSource(int value)
        {
            this.value = value;
        }


        public int AsRandomSeed()
        {
            return value;
        }
    }
}