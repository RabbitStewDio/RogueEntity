namespace RogueEntity.Core.Infrastructure.ItemTraits
{
    public interface ITrait
    {
        ItemTraitId Id { get; }
        int Priority { get; }
    }
}