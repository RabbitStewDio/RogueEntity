using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public sealed class DurabilityBulkTrait<TContext, TItemId> : IItemComponentTrait<TContext, TItemId, Durability>,
                                                                 IBulkDataTrait<TContext, TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly Durability baseValue;

        public DurabilityBulkTrait(ushort maxDurability) : this(maxDurability, maxDurability)
        {
        }

        public DurabilityBulkTrait(ushort initialCount, ushort maxDurability)
        {
            Id = "ItemTrait.Generic.Durability";
            Priority = 100;

            this.baseValue = new Durability(initialCount, maxDurability);
        }

        public ItemTraitId Id { get; }
        public int Priority { get; }

        public TItemId Initialize(TContext context, IItemDeclaration item, TItemId reference)
        {
            return reference.WithData(baseValue.HitPoints);
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TContext context, TItemId k, out Durability t)
        {
            if (k.IsReference)
            {
                return v.GetComponent(k, out t);
            }

            t = baseValue.WithHitPoints(k.Data);
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v,
                              TContext context,
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

        public bool TryRemove(IEntityViewControl<TItemId> entityRegistry, TContext context, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }

        IBulkItemTrait<TContext, TItemId> IBulkItemTrait<TContext, TItemId>.CreateInstance()
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