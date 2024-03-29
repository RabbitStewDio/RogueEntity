using System;

namespace RogueEntity.Core.Runtime
{
    [Flags]
    public enum GameStatus
    {
        // @formatter:off
        NotStarted =   0b00000,
        Initialized =  0b00001,
        Running =      0b00010,
        GameWon =      0b00100,
        GameLost =     0b01000,
        Error =        0b10000,
        // @formatter:on
    }

    public static class GameStatusMasks
    {
        public const GameStatus FinishedMask = GameStatus.Error | GameStatus.GameWon | GameStatus.GameLost;
        public const GameStatus GameActiveMask = GameStatus.Running | GameStatus.GameWon | GameStatus.GameLost;

        public static bool IsFinished(this GameStatus g) => (g & FinishedMask) != 0;
    }
}
