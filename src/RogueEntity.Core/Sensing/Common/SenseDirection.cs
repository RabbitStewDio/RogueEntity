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
        West = 8
    }
}