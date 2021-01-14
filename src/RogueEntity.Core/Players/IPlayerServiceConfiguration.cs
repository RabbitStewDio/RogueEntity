using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Players
{
    public interface IPlayerServiceConfiguration
    {
        ItemDeclarationId PlayerId { get; }
    }
}
