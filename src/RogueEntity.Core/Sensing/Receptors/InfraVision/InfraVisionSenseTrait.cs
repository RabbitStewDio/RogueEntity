using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    public class InfraVisionSenseTrait< TActorId> : SenseReceptorTraitBase< TActorId, VisionSense, TemperatureSense>,
                                                                 IItemComponentInformationTrait< TActorId, IHeatMap>
        where TActorId : struct, IEntityKey
    {
        readonly IInfraVisionSenseReceptorPhysicsConfiguration physics;

        public InfraVisionSenseTrait(IInfraVisionSenseReceptorPhysicsConfiguration physics, float senseIntensity, bool active = true): base(physics.InfraVisionPhysics, senseIntensity, active)
        {
            this.physics = physics;
        }

        public override ItemTraitId Id => "Core.Sense.Receptor.InfraVision";
        public override int Priority => 200;

        public bool TryQuery(IEntityViewControl<TActorId> v, TActorId k, [MaybeNullWhen(false)] out IHeatMap t)
        {
            if (v.GetComponent(k, out SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense> d))
            {
                t = new SingleLevelInfraVisionMap(physics, d);
                return true;
            }

            t = default;
            return false;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return InfraVisionSenseModule.SenseReceptorActorRole.Instantiate<TActorId>();
        }
    }
}