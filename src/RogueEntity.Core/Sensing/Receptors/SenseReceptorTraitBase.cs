using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Receptors
{
    public abstract class SenseReceptorTraitBase<TGameContext, TActorId, TSense, TSource> : IReferenceItemTrait<TGameContext, TActorId>,
                                                                                            IItemComponentTrait<TGameContext, TActorId, SensoryReceptorData<TSense>>
        where TActorId : IEntityKey
        where TSense : ISense
    {
        readonly ISensePhysics physics;
        protected readonly float Intensity;

        protected SenseReceptorTraitBase([NotNull] ISensePhysics physics, float intensity)
        {
            this.physics = physics ?? throw new ArgumentNullException(nameof(physics));
            this.Intensity = intensity;
        }
        
        public abstract string Id { get; }
        public abstract int Priority { get; }

        public virtual void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            v.AssignComponent(k, new SensoryReceptorData<TSense>(new SenseSourceDefinition(physics.DistanceMeasurement, Intensity), true));
            v.AssignComponent(k, SensoryReceptorState.Create<TSense>());
            v.AssignComponent(k, SingleLevelSenseDirectionMapData.Create<TSense, TSource>());
        }

        public virtual void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            if (!v.GetComponent(k, out SensoryReceptorData<TSense> _))
            {
                return;
            }

            if (!v.GetComponent(k, out SensoryReceptorState<TSense> s))
            {
                v.AssignComponent(k, SensoryReceptorState.Create<TSense>());
            }

            if (!v.GetComponent(k, out SingleLevelSenseDirectionMapData<TSense, TSource> m))
            {
                v.AssignComponent(k, SingleLevelSenseDirectionMapData.Create<TSense, TSource>());
            }
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out SensoryReceptorData<TSense> t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in SensoryReceptorData<TSense> t, out TActorId changedK)
        {
            changedK = k;
            if (v.GetComponent(k, out SensoryReceptorData<TSense> existing))
            {
                if (existing == t)
                {
                    return true;
                }
            }

            v.AssignComponent(k, t);
            if (!v.GetComponent(k, out SensoryReceptorState<TSense> s))
            {
                v.AssignComponent(k, SensoryReceptorState.Create<TSense>());
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