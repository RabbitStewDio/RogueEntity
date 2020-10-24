using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors.InfraVision;

namespace RogueEntity.Core.Sensing.Receptors.Heat
{
    public class HeatDirectionSenseTrait<TGameContext, TActorId> : SenseReceptorTraitBase<TGameContext, TActorId, TemperatureSense, TemperatureSense>,
                                                                   IItemComponentInformationTrait<TGameContext, TActorId, IHeatMap>
        where TActorId : IBulkDataStorageKey<TActorId>
    {
        [NotNull] readonly IHeatPhysicsConfiguration physicsConfiguration;

        public HeatDirectionSenseTrait([NotNull] IHeatPhysicsConfiguration physicsConfiguration,
                                       float intensity) : base(physicsConfiguration.HeatPhysics, intensity)
        {
            this.physicsConfiguration = physicsConfiguration;
        }

        public override string Id => "Core.Sense.Receptor.Heat";
        public override int Priority => 100;

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out IHeatMap t)
        {
            if (v.GetComponent(k, out SingleLevelSenseDirectionMapData<TemperatureSense, TemperatureSense> m))
            {
                t = new SingleLevelHeatDirectionMap(physicsConfiguration, m);
                return true;
            }

            t = default;
            return false;
        }
    }
}