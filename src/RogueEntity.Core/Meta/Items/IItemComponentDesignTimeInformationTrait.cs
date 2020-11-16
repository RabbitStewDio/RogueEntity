using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public interface IItemComponentDesignTimeInformationTrait<TComponent> : IItemTrait
    {
        bool TryQuery(out TComponent t);
    }
}