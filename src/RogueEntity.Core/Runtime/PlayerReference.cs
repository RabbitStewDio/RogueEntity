using RogueEntity.Core.Players;

namespace RogueEntity.Core.Runtime
{
    public readonly struct PlayerReference<TPlayerEntity>
    {
        public PlayerTag Tag { get;  }
        public TPlayerEntity EntityId { get; }

        public PlayerReference(PlayerTag tag, TPlayerEntity entityId)
        {
            Tag = tag;
            EntityId = entityId;
        }
    }
}
