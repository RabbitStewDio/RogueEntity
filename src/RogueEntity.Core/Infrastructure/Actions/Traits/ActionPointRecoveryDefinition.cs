namespace RogueEntity.Core.Infrastructure.Actions.Traits
{
    public readonly struct ActionPointRecoveryDefinition
    {
        public readonly int Frequency;
        public readonly int Magnitude;

        public ActionPointRecoveryDefinition(int frequency, int magnitude)
        {
            this.Frequency = frequency;
            this.Magnitude = magnitude;
        }
    }
}