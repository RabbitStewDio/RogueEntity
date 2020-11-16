namespace RogueEntity.Api.ItemTraits
{
    public interface ITrait
    {
        ItemTraitId Id { get; }
        int Priority { get; }
    }
}