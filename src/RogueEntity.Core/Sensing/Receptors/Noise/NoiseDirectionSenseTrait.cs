using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Receptors.Noise
{
    public class NoiseDirectionSenseTrait<TGameContext, TActorId> : SenseReceptorTraitBase<TGameContext, TActorId, NoiseSense, NoiseSense>,
                                                                    IItemComponentInformationTrait<TGameContext, TActorId, INoiseDirectionMap>
        where TActorId : IBulkDataStorageKey<TActorId>
    {
        public NoiseDirectionSenseTrait([NotNull] INoisePhysicsConfiguration physicsConfiguration,
                                        float intensity, bool active = true): base(physicsConfiguration.NoisePhysics, intensity, active)
        {
        }

        public override string Id => "Core.Sense.Receptor.Noise";
        public override int Priority => 100;

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out INoiseDirectionMap t)
        {
            if (v.GetComponent(k, out SingleLevelSenseDirectionMapData<NoiseSense, NoiseSense> m))
            {
                t = new SingleLevelNoiseMap(m);
                return true;
            }

            t = default;
            return false;
        }
    }
}