using EnTTSharp;
using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Sources
{
    public abstract class SenseSourceTraitBase<TItemId, TSense, TSenseDefinition> : IReferenceItemTrait<TItemId>,
                                                                                    IItemComponentTrait<TItemId, TSenseDefinition>
        where TItemId : struct, IEntityKey
        where TSenseDefinition : ISenseDefinition
    {
        public abstract ItemTraitId Id { get; }
        public abstract int Priority { get; }

        public IReferenceItemTrait<TItemId> CreateInstance()
        {
            return this;
        }

        protected abstract bool TryGetInitialValue(out TSenseDefinition senseDefinition);

        public void Initialize(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        {
            if (TryGetInitialValue(out var senseDefinition))
            {
                v.AssignComponent(k, senseDefinition);
                v.AssignComponent(k, new SenseSourceState<TSense>(Optional.Empty(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid));
            }
        }

        public void Apply(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        {
            if (!v.GetComponent<TSenseDefinition>(k, out var _))
            {
                return;
            }

            if (!v.GetComponent<SenseSourceState<TSense>>(k, out var s))
            {
                s = new SenseSourceState<TSense>(Optional.Empty(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
                v.AssignComponent(k, in s);
            }
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, [MaybeNullWhen(false)] out TSenseDefinition t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TItemId k, in TSenseDefinition t, out TItemId changedK)
        {
            v.AssignOrReplace(k, t);

            if (!v.GetComponent<SenseSourceState<TSense>>(k, out var s))
            {
                s = new SenseSourceState<TSense>(Optional.Empty(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
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

        public bool TryRemove(IEntityViewControl<TItemId> v, TItemId k, out TItemId changedK)
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
