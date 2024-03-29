using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public class NoiseSourceTrait<TItemId> : SenseSourceTraitBase<TItemId, NoiseSense, NoiseSourceDefinition>,
                                             IItemComponentTrait<TItemId, NoiseClip>
        where TItemId : struct, IEntityKey
    {
        public override ItemTraitId Id => "Core.Item.NoiseSource";
        public override int Priority => 100;

        readonly INoisePhysicsConfiguration physics;

        public NoiseSourceTrait(INoisePhysicsConfiguration physics)
        {
            this.physics = physics ?? throw new ArgumentNullException(nameof(physics));
        }

        protected override bool TryGetInitialValue(out NoiseSourceDefinition senseDefinition)
        {
            senseDefinition = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v,  TItemId k, out NoiseClip t)
        {
            if (v.GetComponent(k, out NoiseSourceDefinition d))
            {
                t = d.Clip;
                return true;
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TItemId k, in NoiseClip t, out TItemId changedK)
        {
            return TryUpdate(v, k, new NoiseSourceDefinition(new SenseSourceDefinition(physics.NoisePhysics.DistanceMeasurement,
                                                                                                physics.NoisePhysics.AdjacencyRule,
                                                                                                t.Intensity), t, true), out changedK);
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return SenseSourceModules.GetSourceRole<NoiseSense>().Instantiate<TItemId>();
        }
    }
}
