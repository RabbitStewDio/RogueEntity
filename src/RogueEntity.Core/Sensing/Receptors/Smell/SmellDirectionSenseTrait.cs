using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Receptors.Smell
{
    public class SmellDirectionSenseTrait<TActorId> : SenseReceptorTraitBase<TActorId, SmellSense, SmellSense>,
                                                      IItemComponentInformationTrait<TActorId, ISmellDirectionMap>
        where TActorId : struct, IEntityKey
    {
        public SmellDirectionSenseTrait(ISmellSenseReceptorPhysicsConfiguration physicsConfiguration,
                                        float intensity,
                                        bool active = true) : base(physicsConfiguration.SmellPhysics, intensity, active)
        { }

        public override ItemTraitId Id => "Core.Sense.Receptor.Smell";
        public override int Priority => 100;

        public bool TryQuery(IEntityViewControl<TActorId> v, TActorId k, [MaybeNullWhen(false)] out ISmellDirectionMap t)
        {
            if (v.GetComponent(k, out SingleLevelSenseDirectionMapData<SmellSense, SmellSense> m))
            {
                t = new SingleLevelSmellDirectionMap(m);
                return true;
            }

            t = default;
            return false;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return SmellDirectionSenseModule.SenseReceptorActorRole.Instantiate<TActorId>();
        }
    }
}
