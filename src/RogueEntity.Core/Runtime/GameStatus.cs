using System;

namespace RogueEntity.Core.Runtime
{
    [Flags]
    public enum GameStatus
    {
        // @formatter:off
        NotStarted =   0b00000,
        Initialized =  0b00001,
        Running =      0b00011,
        GameLost =     0b01001,
        GameWon =      0b00101,
        Error =        0b10001,
        FinishedMask = 0b11100,
        // @formatter:on
    }
}
