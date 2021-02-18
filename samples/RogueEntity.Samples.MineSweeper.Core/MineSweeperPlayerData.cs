using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Samples.MineSweeper.Core
{
    public readonly struct MineSweeperPlayerData
    {
        public readonly Optional<EntityGridPosition> ExplodedPosition;
        public readonly bool AreaCleared;

        public MineSweeperPlayerData(Optional<EntityGridPosition> explodedPosition, bool areaCleared = false)
        {
            ExplodedPosition = explodedPosition;
            AreaCleared = areaCleared;
        }

        public MineSweeperPlayerData WithExplodedPosition(EntityGridPosition pos)
        {
            return new MineSweeperPlayerData(pos);
        }

        public MineSweeperPlayerData WithAreaCleared()
        {
            return new MineSweeperPlayerData(this.ExplodedPosition, true);
        }
    }
}
