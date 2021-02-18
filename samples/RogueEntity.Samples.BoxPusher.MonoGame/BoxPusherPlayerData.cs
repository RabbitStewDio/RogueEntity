using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Players;
using RogueEntity.Samples.BoxPusher.Core.ItemTraits;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherPlayerData
    {
        public BoxPusherPlayerProfile PlayerRecord { get; set; }
        public readonly PlayerTag PlayerTag;
        public readonly ActorReference PlayerEntityId;

        public BoxPusherPlayerData(in PlayerTag playerTag, in ActorReference playerEntityId, BoxPusherPlayerProfile playerRecord)
        {
            PlayerRecord = playerRecord;
            this.PlayerTag = playerTag;
            this.PlayerEntityId = playerEntityId;
        }
    }
}
