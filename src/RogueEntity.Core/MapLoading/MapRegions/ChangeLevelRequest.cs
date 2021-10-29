namespace RogueEntity.Core.MapLoading.MapRegions
{
    public readonly struct ChangeLevelRequest
    {
        public readonly int Level;

        public ChangeLevelRequest(int level)
        {
            Level = level;
        }
    }
}
