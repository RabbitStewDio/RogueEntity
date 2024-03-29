using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing.Receptors.InfraVision;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Receptors.Heat
{
    public class HeatDirectionSenseTrait< TActorId> : SenseReceptorTraitBase< TActorId, TemperatureSense, TemperatureSense>,
                                                                   IItemComponentInformationTrait< TActorId, IHeatMap>
        where TActorId : struct, IEntityKey
    {
        readonly IHeatSenseReceptorPhysicsConfiguration physicsConfiguration;

        public HeatDirectionSenseTrait(IHeatSenseReceptorPhysicsConfiguration physicsConfiguration,
                                       float intensity,
                                       bool active = true) : base(physicsConfiguration.HeatPhysics, intensity, active)
        {
            this.physicsConfiguration = physicsConfiguration;
        }

        public override ItemTraitId Id => "Core.Sense.Receptor.Heat";
        public override int Priority => 100;

        public bool TryQuery(IEntityViewControl<TActorId> v, TActorId k, [MaybeNullWhen(false)] out IHeatMap t)
        {
            if (v.GetComponent(k, out SingleLevelSenseDirectionMapData<TemperatureSense, TemperatureSense> m))
            {
                t = new SingleLevelHeatDirectionMap(physicsConfiguration, m);
                return true;
            }

            t = default;
            return false;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return HeatDirectionSenseModule.SenseReceptorActorRole.Instantiate<TActorId>();
        }
    }
}