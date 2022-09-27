using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    public class VisionSenseTrait< TActorId> : SenseReceptorTraitBase< TActorId, VisionSense, VisionSense>,
                                                            IItemComponentInformationTrait< TActorId, IBrightnessMap>
        where TActorId : struct, IEntityKey
    {
        public VisionSenseTrait(IVisionSenseReceptorPhysicsConfiguration physics, float senseIntensity, bool active = true): base(physics.VisionPhysics, senseIntensity, active)
        {
        }

        public override ItemTraitId Id => "Core.Sense.Receptor.Vision";
        public override int Priority => 100;

        public bool TryQuery(IEntityViewControl<TActorId> v, TActorId k, [MaybeNullWhen(false)] out IBrightnessMap t)
        {
            if (v.GetComponent(k, out SingleLevelSenseDirectionMapData<VisionSense, VisionSense> m))
            {
                t = new SingleLevelBrightnessMap(m, null);
                return true;
            }

            t = default;
            return false;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return VisionSenseModule.SenseReceptorActorRole.Instantiate<TActorId>();
        }
    }
}