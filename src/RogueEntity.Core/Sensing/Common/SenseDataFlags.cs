using System;

namespace RogueEntity.Core.Sensing.Common
{
    [Flags]
    public enum SenseDataFlags
    {
        None = 0,
        Obstructed = 1,
        SelfIlluminating = 2
    }
}