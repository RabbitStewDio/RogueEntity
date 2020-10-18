using EnTTSharp.Entities;
using GoRogue;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Map.Light;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    public class VisionSenseTrait<TGameContext, TActorId> : IReferenceItemTrait<TGameContext, TActorId>,
                                                            IItemComponentTrait<TGameContext, TActorId, SensoryReceptorData<VisionSense>>,
                                                            IItemComponentInformationTrait<TGameContext, TActorId, IBrightnessMap>
        where TActorId : IBulkDataStorageKey<TActorId>
    {
        readonly SensoryReceptorData<VisionSense> sense;

        public VisionSenseTrait(ILightPhysicsConfiguration physics, float senseIntensity)
        {
            this.sense = new SensoryReceptorData<VisionSense>(new SenseSourceDefinition(physics.LightPhysics.DistanceMeasurement, senseIntensity), true);
        }

        public string Id => "Core.Sense.Receptor.Vision";
        public int Priority => 100;

        public void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            v.AssignComponent(k, sense);
            v.AssignComponent(k, new SensoryReceptorState<VisionSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid));
            v.AssignComponent(k, new SingleLevelBrightnessMap());
        }

        public void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            if (!v.GetComponent(k, out SensoryReceptorData<VisionSense> _))
            {
                return;
            }

            if (!v.GetComponent(k, out SensoryReceptorState<VisionSense> s))
            {
                s = new SensoryReceptorState<VisionSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
                v.AssignComponent(k, in s);
            }

            if (!v.GetComponent(k, out SingleLevelBrightnessMap m))
            {
                m = new SingleLevelBrightnessMap();
                v.AssignComponent(k, m);
            }
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out IBrightnessMap t)
        {
            if (v.GetComponent(k, out SingleLevelBrightnessMap m))
            {
                t = m;
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out SensoryReceptorData<VisionSense> t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in SensoryReceptorData<VisionSense> t, out TActorId changedK)
        {
            changedK = k;
            if (v.GetComponent(k, out SensoryReceptorData<VisionSense> existing))
            {
                if (existing == t)
                {
                    return true;
                }
            }
            
            v.AssignComponent(k, t);
            if (!v.GetComponent(k, out SensoryReceptorState<VisionSense> s))
            {
                s = new SensoryReceptorState<VisionSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
                v.AssignComponent(k, in s);
            }
            else
            {
                s = s.WithDirtyState(SenseSourceDirtyState.UnconditionallyDirty);
                v.AssignComponent(k, in s);
            }
            return true;
        }

        public bool TryRemove(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out TActorId changedK)
        {
            changedK = k;
            return false;
        }
    }
}