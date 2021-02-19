using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Runtime
{
    public static class GameStatusExtensions
    {
        public static bool IsInitialized(this GameStatus s) => s.HasFlags(GameStatus.Initialized);

        public static bool IsStoppable(this GameStatus s) => s.HasFlags(GameStatus.Running) ||
                                                             s.HasFlags(GameStatus.GameLost) ||
                                                             s.HasFlags(GameStatus.GameWon);
    }
}
