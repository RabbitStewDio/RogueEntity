using System;

namespace RogueEntity.Core.Runtime
{
    [Flags]
    public enum GameStatus
    {
        NotStarted = 0b0000,
        Initialized = 0b0001,
        Running = 0b0011,
        GameLost = 0b1001,
        GameWon = 0b0101,
    }
}
