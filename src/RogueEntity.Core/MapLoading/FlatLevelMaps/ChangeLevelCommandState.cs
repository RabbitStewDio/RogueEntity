using EnTTSharp.Entities.Attributes;

namespace RogueEntity.Core.MapLoading.FlatLevelMaps
{
    [EntityComponent]
    public enum ChangeLevelCommandState
    {
        Start = 0,
        PlayerRemoved = 1,
        WaitingForReset = 2,
        WaitingForLoad = 3,
        PlayerPlaced = 4,
        Aborted = 5
    }
}
