using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Sources.Smell
{
    public class SmellSourceTrait<TGameContext, TItemId>: IReferenceItemTrait<TGameContext, TItemId>,
                                                          IItemComponentTrait<TGameContext, TItemId, SmellSource>
        where TItemId : IEntityKey
    {
        public string Id => "Core.Item.SmellSource";
        public int Priority => 100;

        readonly ISmellPhysicsConfiguration physics;

        public SmellSourceTrait([NotNull] ISmellPhysicsConfiguration physics)
        {
            this.physics = physics ?? throw new ArgumentNullException(nameof(physics));
        }

        public void Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        public void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            if (!v.GetComponent(k, out SmellSourceDefinition l))
            {
                return;
            }

            if (!v.GetComponent(k, out SenseSourceState<SmellSense> s))
            {
                s = new SenseSourceState<SmellSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
                v.AssignComponent(k, in s);
            }
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out SmellSource t)
        {
            if (v.GetComponent(k, out SmellSourceDefinition d))
            {
                t = d.Smell;
                return true;
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in SmellSource t, out TItemId changedK)
        {
            v.AssignOrReplace(k, new SmellSourceDefinition(new SenseSourceDefinition(physics.SmellPhysics.DistanceMeasurement, t.Intensity), t, true));

            if (!v.GetComponent(k, out SenseSourceState<SmellSense> s))
            {
                s = new SenseSourceState<SmellSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
                v.AssignComponent(k, in s);
            }
            else
            {
                s = s.WithDirtyState(SenseSourceDirtyState.UnconditionallyDirty);
                v.AssignComponent(k, in s);
            }

            changedK = k;
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TItemId changedK)
        {
            v.RemoveComponent<SmellSourceDefinition>(k);
            changedK = k;
            return true;
        }
    }
}