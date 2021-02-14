using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Players;
using RogueEntity.Simple.BoxPusher.ItemTraits;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    public class PlayerData
    {
        public BoxPusherPlayerProfile PlayerRecord { get; set; }
        public readonly PlayerTag PlayerTag;
        public readonly ActorReference PlayerEntityId;

        public PlayerData(in PlayerTag playerTag, in ActorReference playerEntityId, BoxPusherPlayerProfile playerRecord)
        {
            PlayerRecord = playerRecord;
            this.PlayerTag = playerTag;
            this.PlayerEntityId = playerEntityId;
        }
    }
}
