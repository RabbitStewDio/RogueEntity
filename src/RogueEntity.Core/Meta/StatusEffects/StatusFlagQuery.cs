namespace RogueEntity.Core.Meta.StatusEffects
{
    public readonly struct StatusFlagQuery
    {
        public StatusFlagQuery(long mask)
        {
            this.Mask = mask;
        }

        public long Mask { get; }
    }
}