namespace RogueEntity.Core.Tests.Meta.Items
{
    public interface IItemComponentTestDataFactory<TData>
    {
        public TData ChangedValue { get; }
        public TData OtherChangedValue { get; }

        public bool TryGetInitialValue(out TData initialValue);
        public bool TryGetApplyValue(out TData applyValue);
        public bool TryGetInvalid(out TData invalid);
        public bool TryGetRemoved(out TData removed);
        public bool RemoveAllowed { get; }
        public bool UpdateAllowed { get; }
    }
}