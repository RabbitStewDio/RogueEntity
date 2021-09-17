namespace RogueEntity.Core.MapLoading.MapRegions
{
    public readonly struct ChangeLevelCommand
    {
        public readonly int Level;

        public ChangeLevelCommand(int level)
        {
            Level = level;
        }
    }
}
