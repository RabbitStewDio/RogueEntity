using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public sealed class DurabilityTrait<TContext, TItemId> : IItemComponentTrait<TContext, TItemId, Durability>,
                                                             IReferenceItemTrait<TContext, TItemId>
        where TItemId : IEntityKey
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


        public void Initialize(IEntityViewControl<TItemId> v, TContext context, TItemId k, IItemDeclaration item)
        {
            v.AssignOrReplace(k, in baseValue);
        }

        public void Apply(IEntityViewControl<TItemId> v, TContext context, TItemId k, IItemDeclaration item)
        {
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TContext context, TItemId k, out Durability t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v,
                              TContext context,
                              TItemId k,
                              in Durability t,
                              out TItemId changedK)
        {
            v.AssignOrReplace(k, in t);
            changedK = k;
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> entityRegistry, TContext context, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }

        IReferenceItemTrait<TContext, TItemId> IReferenceItemTrait<TContext, TItemId>.CreateInstance()
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