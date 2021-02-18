using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Players;

namespace RogueEntity.Samples.MineSweeper.MonoGame
{
    public readonly struct PlayerReference
    {
        public PlayerTag Tag { get;  }
        public ActorReference EntityId { get; }

        public PlayerReference(PlayerTag tag, ActorReference entityId)
        {
            Tag = tag;
            EntityId = entityId;
        }
    }
}
