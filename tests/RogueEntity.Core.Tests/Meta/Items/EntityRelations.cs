using EnTTSharp.Entities;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public readonly struct EntityRelations<TItemId>
        where TItemId : IEntityKey
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