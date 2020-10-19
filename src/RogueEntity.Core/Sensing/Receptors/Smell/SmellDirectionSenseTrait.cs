using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Sensing.Receptors.Smell
{
    public class SmellDirectionSenseTrait<TGameContext, TActorId> : SenseReceptorTraitBase<TGameContext, TActorId, SmellSense, SmellSense>,
                                                                    IItemComponentInformationTrait<TGameContext, TActorId, ISmellDirectionMap>
        where TActorId : IBulkDataStorageKey<TActorId>
    {
        public SmellDirectionSenseTrait([NotNull] ISmellPhysicsConfiguration physicsConfiguration,
                                        float intensity) : base(physicsConfiguration.SmellPhysics, intensity)
        {
        }

        public override string Id => "Core.Sense.Receptor.Smell";
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
    }
}