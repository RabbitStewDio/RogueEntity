using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public sealed class DurabilityBulkTrait<TItemId> : IItemComponentTrait<TItemId, Durability>,
                                                       IBulkDataTrait<TItemId>
        where TItemId : struct, IBulkDataStorageKey<TItemId>
    {
        readonly Durability baseValue;

        public DurabilityBulkTrait(ushort maxDurability) : this(maxDurability, maxDurability)
        { }

        public DurabilityBulkTrait(ushort initialCount, ushort maxDurability)
        {
            Id = "ItemTrait.Generic.Durability";
            Priority = 100;

            this.baseValue = new Durability(initialCount, maxDurability);
        }

        public ItemTraitId Id { get; }
        public int Priority { get; }

        public TItemId Initialize(IItemDeclaration item, TItemId reference)
        {
            return reference.WithData(baseValue.HitPoints);
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out Durability t)
        {
            if (k.IsReference)
            {
                return v.GetComponent(k, out t);
            }

            t = baseValue.WithHitPoints(k.Data);
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v,
                              TItemId k,
                              in Durability t,
                              out TItemId changedK)
        {
            if (k.IsReference)
            {
                v.AssignOrReplace(k, in t);
                changedK = k;
                return true;
            }

            changedK = k.WithData(t.HitPoints);
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> entityRegistry, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }

        IBulkItemTrait<TItemId> IBulkItemTrait<TItemId>.CreateInstance()
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
