using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public class ItemComponentTestDataFactory<TData> : IItemComponentTestDataFactory<TData>
    {
        public TData ChangedValue { get; }
        public TData OtherChangedValue { get; }
        readonly Optional<TData> applyValueProvider;
        readonly Optional<TData> invalidValueProvider;
        readonly Optional<TData> removedValueProvider;
        readonly Optional<TData> initialValueProvider;

        public ItemComponentTestDataFactory(TData defaultValue)
        {
            initialValueProvider = defaultValue;
            applyValueProvider = defaultValue;
            RemoveAllowed = false;
            UpdateAllowed = false;
        }

        public ItemComponentTestDataFactory(Optional<TData> defaultValue,
                                            TData changedValue,
                                            TData otherChangedValue)
        {
            initialValueProvider = defaultValue;
            ChangedValue = changedValue;
            applyValueProvider = changedValue;
            OtherChangedValue = otherChangedValue;
            RemoveAllowed = true;
            UpdateAllowed = true;
        }

        ItemComponentTestDataFactory(Optional<TData> defaultValue,
                                     bool updatedValue,
                                     TData changedValue,
                                     Optional<TData> applyValue,
                                     TData otherChangedValue,
                                     Optional<TData> invalidValueProvider,
                                     bool removedResult,
                                     Optional<TData> removedValueProvider)
        {
            initialValueProvider = defaultValue;
            ChangedValue = changedValue;
            applyValueProvider = applyValue;
            OtherChangedValue = otherChangedValue;
            this.UpdateAllowed = updatedValue;
            this.invalidValueProvider = invalidValueProvider;
            this.RemoveAllowed = removedResult;
            this.removedValueProvider = removedValueProvider;
        }

        public ItemComponentTestDataFactory<TData> WithRemoveProhibited()
        {
            return new ItemComponentTestDataFactory<TData>(initialValueProvider, UpdateAllowed, ChangedValue, applyValueProvider, OtherChangedValue, invalidValueProvider, false,
                                                           Optional.Empty());
        }

        public ItemComponentTestDataFactory<TData> WithRemovedResultAsDefault()
        {
            return new ItemComponentTestDataFactory<TData>(initialValueProvider, UpdateAllowed, ChangedValue, applyValueProvider, OtherChangedValue, invalidValueProvider, true, initialValueProvider);
        }

        public ItemComponentTestDataFactory<TData> WithRemovedResult(Optional<TData> removeResult)
        {
            return new ItemComponentTestDataFactory<TData>(initialValueProvider, UpdateAllowed, ChangedValue, applyValueProvider, OtherChangedValue, invalidValueProvider, true, removeResult);
        }

        public ItemComponentTestDataFactory<TData> WithInvalidResult(TData invalid)
        {
            return new ItemComponentTestDataFactory<TData>(initialValueProvider, UpdateAllowed, ChangedValue, applyValueProvider, OtherChangedValue, invalid, RemoveAllowed, removedValueProvider);
        }

        public ItemComponentTestDataFactory<TData> WithoutInvalidResult()
        {
            return new ItemComponentTestDataFactory<TData>(initialValueProvider, UpdateAllowed, ChangedValue, applyValueProvider, OtherChangedValue, Optional.Empty(), RemoveAllowed,
                                                           removedValueProvider);
        }

        public ItemComponentTestDataFactory<TData> WithApplyRestoresDefaultValue()
        {
            return new ItemComponentTestDataFactory<TData>(initialValueProvider, UpdateAllowed, ChangedValue, initialValueProvider, OtherChangedValue, invalidValueProvider, RemoveAllowed,
                                                           removedValueProvider);
        }

        public ItemComponentTestDataFactory<TData> WithApplyResult(TData applyResult)
        {
            return new ItemComponentTestDataFactory<TData>(initialValueProvider, UpdateAllowed, ChangedValue, applyResult, OtherChangedValue, invalidValueProvider, RemoveAllowed,
                                                           removedValueProvider);
        }

        public ItemComponentTestDataFactory<TData> WithUpdateProhibited()
        {
            return new ItemComponentTestDataFactory<TData>(initialValueProvider, false, ChangedValue, applyValueProvider, OtherChangedValue, invalidValueProvider, RemoveAllowed, removedValueProvider);
        }

        public bool UpdateAllowed { get; }
        public bool RemoveAllowed { get; }

        public bool TryGetRemoved(out TData removed)
        {
            return removedValueProvider.TryGetValue(out removed);
        }

        public bool TryGetInvalid(out TData invalid)
        {
            return invalidValueProvider.TryGetValue(out invalid);
        }

        public bool TryGetInitialValue(out TData initialValue)
        {
            return initialValueProvider.TryGetValue(out initialValue);
        }

        public bool TryGetApplyValue(out TData applyValue)
        {
            return applyValueProvider.TryGetValue(out applyValue);
        }
    }
}