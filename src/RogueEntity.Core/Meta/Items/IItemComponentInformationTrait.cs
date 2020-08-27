namespace RogueEntity.Core.Meta.Items
{
    public interface IItemComponentInformationTrait<out TData>: IItemTrait
    {
        TData BaseValue { get; }
    }
}