namespace RogueEntity.Core.Infrastructure.Meta.Items
{
    public interface IItemComponentInformationTrait<out TData>: IItemTrait
    {
        TData BaseValue { get; }
    }
}