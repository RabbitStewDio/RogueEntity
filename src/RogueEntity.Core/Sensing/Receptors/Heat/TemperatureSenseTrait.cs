using System;
using EnTTSharp.Entities;
using GoRogue;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Receptors.Heat
{
    public class TemperatureSenseTrait<TGameContext, TActorId> : IReferenceItemTrait<TGameContext, TActorId>,
                                                                 IItemComponentTrait<TGameContext, TActorId, SensoryReceptorData<VisionSense>>
        where TActorId : IBulkDataStorageKey<TActorId>
    {
        readonly ILightPhysicsConfiguration physics;
        readonly SensoryReceptorData<VisionSense> sense;

        public TemperatureSenseTrait([NotNull] ILightPhysicsConfiguration physics, float senseIntensity, float radius = 0)
        {
            this.physics = physics ?? throw new ArgumentNullException(nameof(physics));
            if (radius <= 0)
            {
                radius = physics.LightPhysics.SignalRadiusForIntensity(senseIntensity);
            }

            this.sense = new SensoryReceptorData<VisionSense>(new SenseSourceDefinition(DistanceCalculation.EUCLIDEAN, senseIntensity), true);
        }

        public string Id => "Core.Sense.Receptor.Vision";
        public int Priority => 100;

        public void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            v.AssignComponent(k, sense);
            v.AssignComponent(k, new SensoryReceptorState<VisionSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid));
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