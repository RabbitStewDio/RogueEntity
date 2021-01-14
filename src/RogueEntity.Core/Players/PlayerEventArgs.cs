namespace RogueEntity.Core.Players
{
    public readonly struct PlayerEventArgs<TEntityId>
    {
        public readonly PlayerTag PlayerId;
        public readonly TEntityId EntityId;

        public PlayerEventArgs(PlayerTag playerId, TEntityId entityId)
        {
            PlayerId = playerId;
            EntityId = entityId;
        }
    }
}
