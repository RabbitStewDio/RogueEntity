using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public class ItemComponentTestDataFactory<TData> : IItemComponentTestDataFactory<TData>
    {
        public TData DefaultValue { get; }
        public TData ChangedValue { get; }
        public TData ApplyValue { get; }
        public TData OtherChangedValue { get; }
        readonly Optional<TData> invalidValueProvider;
        readonly Optional<TData> removedValueProvider;
        readonly bool removedResult;
        readonly bool updatedResult;

        public ItemComponentTestDataFactory(TData defaultValue, 
                                            TData changedValue, 
                                            TData otherChangedValue)
        {
            DefaultValue = defaultValue;
            ChangedValue = changedValue;
            ApplyValue = ChangedValue;
            OtherChangedValue = otherChangedValue;
            removedResult = true;
            updatedResult = true;
        }

        public ItemComponentTestDataFactory(TData defaultValue, 
                                            bool updatedValue,
                                            TData changedValue, 
                                            TData applyValue, 
                                            TData otherChangedValue, 
                                            Optional<TData> invalidValueProvider,
                                            bool removedResult,
                                            Optional<TData> removedValueProvider)
        {
            DefaultValue = defaultValue;
            ChangedValue = changedValue;
            ApplyValue = applyValue;
            OtherChangedValue = otherChangedValue;
            this.updatedResult = updatedValue;
            this.invalidValueProvider = invalidValueProvider;
            this.removedResult = removedResult;
            this.removedValueProvider = removedValueProvider;
        }

        public ItemComponentTestDataFactory<TData> WithRemoveProhibited()
        {
            return new ItemComponentTestDataFactory<TData>(DefaultValue, updatedResult, ChangedValue, ApplyValue, OtherChangedValue, invalidValueProvider, false, Optional.Empty<TData>());
        }

        public ItemComponentTestDataFactory<TData> WithRemovedResultAsDefault()
        {
            return new ItemComponentTestDataFactory<TData>(DefaultValue, updatedResult, ChangedValue, ApplyValue, OtherChangedValue, invalidValueProvider, true, DefaultValue);
        }

        public ItemComponentTestDataFactory<TData> WithRemovedResult(TData removeResult)
        {
            return new ItemComponentTestDataFactory<TData>(DefaultValue, updatedResult, ChangedValue, ApplyValue, OtherChangedValue, invalidValueProvider, true, removeResult);
        }

        public ItemComponentTestDataFactory<TData> WithInvalidResult(TData invalid)
        {
            return new ItemComponentTestDataFactory<TData>(DefaultValue, updatedResult, ChangedValue, ApplyValue, OtherChangedValue, invalid, removedResult, removedValueProvider);
        }

        public ItemComponentTestDataFactory<TData> WithApplyRestoresDefaultValue()
        {
            return new ItemComponentTestDataFactory<TData>(DefaultValue, updatedResult, ChangedValue, DefaultValue, OtherChangedValue, invalidValueProvider, removedResult, removedValueProvider);
        }

        public ItemComponentTestDataFactory<TData> WithApplyResult(TData applyResult)
        {
            return new ItemComponentTestDataFactory<TData>(DefaultValue, updatedResult, ChangedValue, applyResult, OtherChangedValue, invalidValueProvider, removedResult, removedValueProvider);
        }

        public ItemComponentTestDataFactory<TData> WithUpdateProhibited()
        {
            return new ItemComponentTestDataFactory<TData>(DefaultValue, false, ChangedValue, ApplyValue, OtherChangedValue, invalidValueProvider, removedResult, removedValueProvider);
        }

        public bool UpdateAllowed => updatedResult;
        public bool RemoveAllowed => removedResult;

        public bool TryGetRemoved(out TData removed)
        {
            return removedValueProvider.TryGetValue(out removed);
        }

        public bool TryGetInvalid(out TData invalid)
        {
            return invalidValueProvider.TryGetValue(out invalid);
        }
    }
}