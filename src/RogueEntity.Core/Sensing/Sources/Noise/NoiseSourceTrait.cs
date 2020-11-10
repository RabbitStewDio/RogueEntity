using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public class NoiseSourceTrait<TGameContext, TItemId> : SenseSourceTraitBase<TGameContext, TItemId, NoiseSense, NoiseSourceDefinition>,
                                                           IItemComponentTrait<TGameContext, TItemId, NoiseClip>
        where TItemId : IEntityKey
    {
        public override string Id => "Core.Item.NoiseSource";
        public override int Priority => 100;

        readonly INoisePhysicsConfiguration physics;

        public NoiseSourceTrait([NotNull] INoisePhysicsConfiguration physics)
        {
            this.physics = physics ?? throw new ArgumentNullException(nameof(physics));
        }

        protected override bool TryGetInitialValue(out NoiseSourceDefinition senseDefinition)
        {
            senseDefinition = default;
            return false;
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
            return TryUpdate(v, context, k, new NoiseSourceDefinition(new SenseSourceDefinition(physics.NoisePhysics.DistanceMeasurement,
                                                                                                physics.NoisePhysics.AdjacencyRule,
                                                                                                t.Intensity), t, true), out changedK);
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return SenseSourceModules.GetSourceRole<NoiseSense>().Instantiate<TItemId>();
        }
    }
}