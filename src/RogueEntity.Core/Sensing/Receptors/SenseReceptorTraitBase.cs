using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Receptors
{
    public abstract class SenseReceptorTraitBase<TGameContext, TActorId, TReceptorSense, TSourceSense> : IReferenceItemTrait<TGameContext, TActorId>,
                                                                                                         IItemComponentTrait<TGameContext, TActorId, SensoryReceptorData<TReceptorSense, TSourceSense>>
        where TActorId : IEntityKey
        where TReceptorSense : ISense
        where TSourceSense : ISense
    {
        readonly ISensePhysics physics;
        protected readonly float Intensity;
        readonly bool active;

        protected SenseReceptorTraitBase([NotNull] ISensePhysics physics, float intensity, bool active)
        {
            this.physics = physics ?? throw new ArgumentNullException(nameof(physics));
            this.Intensity = intensity;
            this.active = active;
        }

        public abstract string Id { get; }
        public abstract int Priority { get; }

        public virtual void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            v.AssignComponent(k, new SensoryReceptorData<TReceptorSense, TSourceSense>(new SenseSourceDefinition(physics.DistanceMeasurement, physics.AdjacencyRule, Intensity), active));
            v.AssignComponent(k, SensoryReceptorState.Create<TReceptorSense, TSourceSense>());
            v.AssignComponent(k, SingleLevelSenseDirectionMapData.Create<TReceptorSense, TSourceSense>());
        }

        public virtual void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            if (!v.GetComponent(k, out SensoryReceptorData<TReceptorSense, TSourceSense> _))
            {
                return;
            }

            if (!v.GetComponent(k, out SensoryReceptorState<TReceptorSense, TSourceSense> _))
            {
                v.AssignComponent(k, SensoryReceptorState.Create<TReceptorSense, TSourceSense>());
            }

            if (!v.GetComponent(k, out SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense> _))
            {
                v.AssignComponent(k, SingleLevelSenseDirectionMapData.Create<TReceptorSense, TSourceSense>());
            }
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out SensoryReceptorData<TReceptorSense, TSourceSense> t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in SensoryReceptorData<TReceptorSense, TSourceSense> t, out TActorId changedK)
        {
            changedK = k;
            if (v.GetComponent(k, out SensoryReceptorData<TReceptorSense, TSourceSense> existing))
            {
                if (existing == t)
                {
                    return true;
                }
            }

            v.AssignOrReplace(k, t);
            if (!v.GetComponent(k, out SensoryReceptorState<TReceptorSense, TSourceSense> s))
            {
                v.AssignComponent(k, SensoryReceptorState.Create<TReceptorSense, TSourceSense>());
            }
            else
            {
                s = s.WithDirtyState(SenseSourceDirtyState.UnconditionallyDirty);
                v.ReplaceComponent(k, in s);
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