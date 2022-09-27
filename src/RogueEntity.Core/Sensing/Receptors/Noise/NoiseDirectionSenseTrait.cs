using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Receptors.Noise
{
    public class NoiseDirectionSenseTrait<TActorId> : SenseReceptorTraitBase<TActorId, NoiseSense, NoiseSense>,
                                                      IItemComponentInformationTrait<TActorId, INoiseDirectionMap>
        where TActorId : struct, IEntityKey
    {
        public NoiseDirectionSenseTrait(INoiseSenseReceptorPhysicsConfiguration physicsConfiguration,
                                        float intensity,
                                        bool active = true) : base(physicsConfiguration.NoisePhysics, intensity, active)
        {
        }

        public override ItemTraitId Id => "Core.Sense.Receptor.Noise";
        public override int Priority => 100;

        public bool TryQuery(IEntityViewControl<TActorId> v, TActorId k, [MaybeNullWhen(false)] out INoiseDirectionMap t)
        {
            if (v.GetComponent(k, out SingleLevelSenseDirectionMapData<NoiseSense, NoiseSense> m))
            {
                t = new SingleLevelNoiseMap(m);
                return true;
            }

            t = default;
            return false;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return NoiseDirectionSenseModule.SenseReceptorActorRole.Instantiate<TActorId>();
        }
    }
}