using EnTTSharp.Entities.Attributes;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Simple.MineSweeper
{
    [EntityComponent()]
    public class MineSweeperPlayerData
    {
        public PlayerTag PlayerTag { get; }
        public ActorReference PlayerEntityId { get; }
        public MineSweeperPlayerProfile PlayerRecord { get; set; }
        public int Seed => PlayerRecord.Seed;
        
        public Dimension PlayField;
        public int MineCount;
        public int UndiscoveredTileCount;
        public Optional<EntityGridPosition> ExplodedPosition;
        
        public DynamicBoolDataView DiscoveredArea;

        public MineSweeperPlayerData(PlayerTag playerTag, ActorReference playerEntityId, MineSweeperPlayerProfile playerRecord)
        {
            PlayerTag = playerTag;
            PlayerEntityId = playerEntityId;
            PlayerRecord = playerRecord;
            PlayField = PlayerRecord.PlayFieldArea;
            MineCount = PlayerRecord.MineCount;
        }

        public Rectangle ActiveArea => new Rectangle(1, 1, PlayField.Width, PlayField.Height);
        
        public bool IsGameWon()
        {
            return !ExplodedPosition.HasValue && MineCount == UndiscoveredTileCount;
        }
    }
}
