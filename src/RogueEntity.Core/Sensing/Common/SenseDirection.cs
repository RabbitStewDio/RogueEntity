using System;

namespace RogueEntity.Core.Sensing.Common
{
    [Flags]
    public enum SenseDirection
    {
        None = 0, 
        North = 1, 
        East = 2, 
        South = 4, 
        West = 8,
        
        NorthEast = North| East, // 3
        NorthWest = North| West, // 9
        SouthEast = South| East, // 6
        SouthWest = South| West, // 12
    }
}