using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public class NoiseSourceTrait<TGameContext, TItemId>: IReferenceItemTrait<TGameContext, TItemId>,
                                                          IItemComponentTrait<TGameContext, TItemId, NoiseClip>
        where TItemId : IEntityKey
    {
        public string Id => "Core.Item.NoiseSource";
        public int Priority => 100;

        readonly INoisePhysicsConfiguration physics;

        public NoiseSourceTrait([NotNull] INoisePhysicsConfiguration physics)
        {
            this.physics = physics ?? throw new ArgumentNullException(nameof(physics));
        }

        public void Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        public void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            if (!v.GetComponent(k, out NoiseSourceDefinition l))
            {
                return;
            }

            if (!v.GetComponent(k, out SenseSourceState<NoiseSense> s))
            {
                s = new SenseSourceState<NoiseSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
                v.AssignComponent(k, in s);
            }
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out NoiseClip t)
        {
            if (v.GetComponent(k, out NoiseSourceDefinition d))
            {
                t = d.Clip;
                return true;
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in NoiseClip t, out TItemId changedK)
        {
            v.AssignOrReplace(k, new NoiseSourceDefinition(new SenseSourceDefinition(physics.NoisePhysics.DistanceMeasurement, t.Intensity), t, true));

            if (!v.GetComponent(k, out SenseSourceState<NoiseSense> s))
            {
                s = new SenseSourceState<NoiseSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
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
            v.RemoveComponent<NoiseSourceDefinition>(k);
            changedK = k;
            return true;
        }
    }
}