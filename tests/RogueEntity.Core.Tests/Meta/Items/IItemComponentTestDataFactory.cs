using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public interface IItemComponentTestDataFactory<TData>
    {
        public TData DefaultValue { get; }
        public TData ChangedValue { get; }
        public TData ApplyValue { get; }
        public TData OtherChangedValue { get; }

        public bool TryGetInvalid(out TData invalid);
        public bool TryGetRemoved(out TData invalid);
        public bool RemoveAllowed { get; }
        public bool UpdateAllowed { get; }
    }

    public readonly struct EntityRelations<TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        public TItemId DefaultEntityId { get; }
        public TItemId AlternativeEntityId1 { get; }
        public TItemId AlternativeEntityId2 { get; }

        public EntityRelations(TItemId defaultEntityId, TItemId alternativeEntityId1 = default, TItemId alternativeEntityId2 = default)
        {
            DefaultEntityId = defaultEntityId;
            AlternativeEntityId1 = alternativeEntityId1;
            AlternativeEntityId2 = alternativeEntityId2;
        }
    }
}