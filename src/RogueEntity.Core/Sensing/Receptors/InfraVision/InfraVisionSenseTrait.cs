using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    public class InfraVisionSenseTrait<TGameContext, TActorId> : SenseReceptorTraitBase<TGameContext, TActorId, VisionSense, TemperatureSense>,
                                                                 IItemComponentInformationTrait<TGameContext, TActorId, IHeatMap>
        where TActorId : IEntityKey
    {
        readonly IHeatPhysicsConfiguration physics;

        public InfraVisionSenseTrait(IHeatPhysicsConfiguration physics, float senseIntensity): base(physics.HeatPhysics, senseIntensity)
        {
            this.physics = physics;
        }

        public override string Id => "Core.Sense.Receptor.InfraVision";
        public override int Priority => 200;

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out IHeatMap t)
        {
            if (v.GetComponent(k, out SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense> d))
            {
                t = new SingleLevelInfraVisionMap(physics, d);
                return true;
            }

            t = default;
            return false;
        }
    }
}