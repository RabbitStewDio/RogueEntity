using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    public class VisionSenseTrait<TGameContext, TActorId> : SenseReceptorTraitBase<TGameContext, TActorId, VisionSense, VisionSense>,
                                                            IItemComponentInformationTrait<TGameContext, TActorId, IBrightnessMap>
        where TActorId : IEntityKey
    {
        public VisionSenseTrait(ILightPhysicsConfiguration physics, float senseIntensity): base(physics.LightPhysics, senseIntensity)
        {
        }

        public override string Id => "Core.Sense.Receptor.Vision";
        public override int Priority => 100;

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out IBrightnessMap t)
        {
            if (v.GetComponent(k, out SingleLevelSenseDirectionMapData<VisionSense, VisionSense> m))
            {
                t = new SingleLevelBrightnessMap(m, null);
                return true;
            }

            t = default;
            return false;
        }
    }
}