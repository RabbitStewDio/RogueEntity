using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Players
{
    public interface IPlayerServiceConfiguration
    {
        ItemDeclarationId PlayerId { get; }
    }

    public class PlayerServiceConfiguration : IPlayerServiceConfiguration
    {
        public PlayerServiceConfiguration(ItemDeclarationId playerId)
        {
            PlayerId = playerId;
        }

        public ItemDeclarationId PlayerId { get; }
    }
}
