using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Sources
{
    public abstract class SenseSourceTraitBase<TGameContext, TItemId, TSense, TSenseDefinition> : IReferenceItemTrait<TGameContext, TItemId>,
                                                                                                  IItemComponentTrait<TGameContext, TItemId, TSenseDefinition>
        where TItemId : IEntityKey
        where TSenseDefinition : ISenseDefinition
    {
        public abstract ItemTraitId Id { get; }
        public abstract int Priority { get; }

        public IReferenceItemTrait<TGameContext, TItemId> CreateInstance()
        {
            return this;
        }

        protected abstract bool TryGetInitialValue(out TSenseDefinition senseDefinition);

        public void Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            if (TryGetInitialValue(out var senseDefinition))
            {
                v.AssignComponent(k, senseDefinition);
                v.AssignComponent(k, new SenseSourceState<TSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid));
            }
        }

        public void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            if (!v.GetComponent(k, out TSenseDefinition _))
            {
                return;
            }

            if (!v.GetComponent(k, out SenseSourceState<TSense> s))
            {
                s = new SenseSourceState<TSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
                v.AssignComponent(k, in s);
            }
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TSenseDefinition t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in TSenseDefinition t, out TItemId changedK)
        {
            v.AssignOrReplace(k, t);

            if (!v.GetComponent(k, out SenseSourceState<TSense> s))
            {
                s = new SenseSourceState<TSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
                v.AssignComponent(k, in s);
            }
            else
            {
                s = s.WithDirtyState(SenseSourceDirtyState.UnconditionallyDirty);
                v.ReplaceComponent(k, in s);
            }

            changedK = k;
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TItemId changedK)
        {
            v.RemoveComponent<TSenseDefinition>(k);
            v.RemoveComponent<SenseSourceState<TSense>>(k);
            changedK = k;
            return true;
        }

        public abstract IEnumerable<EntityRoleInstance> GetEntityRoles();

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}