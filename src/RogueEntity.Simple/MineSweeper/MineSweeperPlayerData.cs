using EnTTSharp.Entities.Attributes;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Simple.MineSweeper
{
    [EntityComponent()]
    public class MineSweeperPlayerData
    {
        public Dimension PlayField;
        public int MineCount;
        public int UndiscoveredTileCount;
        public Optional<EntityGridPosition> ExplodedPosition;
        public int Seed;
        public DynamicBoolDataView DiscoveredArea;

        public Rectangle ActiveArea => new Rectangle(1, 1, PlayField.Width, PlayField.Height);
        
        public bool IsGameWon()
        {
            return !ExplodedPosition.HasValue && MineCount == UndiscoveredTileCount;
        }
    }

    public class MineSweeperPlayerProfile
    {
        
    }
}
