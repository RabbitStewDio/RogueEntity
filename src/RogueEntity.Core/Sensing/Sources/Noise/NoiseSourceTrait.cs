using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing.Common;

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