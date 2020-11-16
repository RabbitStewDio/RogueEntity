using System.Collections.Generic;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Sensing.Receptors.Smell
{
    public class SmellDirectionSenseTrait<TGameContext, TActorId> : SenseReceptorTraitBase<TGameContext, TActorId, SmellSense, SmellSense>,
                                                                    IItemComponentInformationTrait<TGameContext, TActorId, ISmellDirectionMap>
        where TActorId : IEntityKey
    {
        public SmellDirectionSenseTrait([NotNull] ISmellSenseReceptorPhysicsConfiguration physicsConfiguration,
                                        float intensity, bool active = true) : base(physicsConfiguration.SmellPhysics, intensity, active)
        {
        }

        public override ItemTraitId Id => "Core.Sense.Receptor.Smell";
        public override int Priority => 100;

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out ISmellDirectionMap t)
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