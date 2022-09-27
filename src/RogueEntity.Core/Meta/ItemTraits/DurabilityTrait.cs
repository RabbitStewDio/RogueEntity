using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public sealed class DurabilityTrait< TItemId> : IItemComponentTrait< TItemId, Durability>,
                                                             IReferenceItemTrait< TItemId>
        where TItemId : struct, IEntityKey
    {
        readonly Durability baseValue;

        public DurabilityTrait(ushort maxDurability) : this(maxDurability, maxDurability)
        {
        }

        public DurabilityTrait(ushort initialCount, ushort maxDurability)
        {
            Id = "ItemTrait.Generic.Durability";
            Priority = 100;

            this.baseValue = new Durability(initialCount, maxDurability);
        }

        public ItemTraitId Id { get; }
        public int Priority { get; }


        public void Initialize(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        {
            v.AssignOrReplace(k, in baseValue);
        }

        public void Apply(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        {
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out Durability t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v,
                              TItemId k,
                              in Durability t,
                              out TItemId changedK)
        {
            v.AssignOrReplace(k, in t);
            changedK = k;
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> entityRegistry, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }

        IReferenceItemTrait< TItemId> IReferenceItemTrait< TItemId>.CreateInstance()
        {
            return this;
        }

        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CoreModule.ItemRole.Instantiate<TItemId>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}