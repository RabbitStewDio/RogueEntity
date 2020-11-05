using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Touch;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Sensing.Receptors.Touch
{
    public class TouchReceptorTrait<TGameContext, TActorId>: SenseReceptorTraitBase<TGameContext, TActorId, TouchSense, TouchSense>,
                                                             IItemComponentInformationTrait<TGameContext, TActorId, ITouchDirectionMap>,
                                                             IItemComponentTrait<TGameContext, TActorId, TouchSourceDefinition>
        where TActorId : IEntityKey
    {
        readonly ITouchReceptorPhysicsConfiguration touchPhysics;

        public TouchReceptorTrait([NotNull] ITouchReceptorPhysicsConfiguration touchPhysics, bool active = true) : base(touchPhysics.TouchPhysics, GetStandardIntensity(touchPhysics), active)
        {
            this.touchPhysics = touchPhysics;
        }

        static float GetStandardIntensity(ITouchReceptorPhysicsConfiguration p) => p.TouchPhysics.DistanceMeasurement.MaximumStepDistance();
        
        public override string Id => "Core.Sense.Receptor.Touch";
        public override int Priority => 100;

        public override void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            base.Initialize(v, context, k, item);
            
            v.AssignComponent(k, new TouchSourceDefinition(new SenseSourceDefinition(touchPhysics.TouchPhysics.DistanceMeasurement,
                                                                                     touchPhysics.TouchPhysics.AdjacencyRule,
                                                                                     Intensity), true));
            v.AssignComponent(k, new SenseSourceState<TouchSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid));
        }

        public override void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            if (v.HasComponent<TouchSourceDefinition>(k) && !v.HasComponent<SenseSourceState<TouchSense>>(k))
            {
                v.AssignComponent(k, new SenseSourceState<TouchSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid));
            }
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out TouchSourceDefinition t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in TouchSourceDefinition t, out TActorId changedK)
        {
            v.AssignOrReplace(k, in t);
            changedK = k;
            
            if (!v.GetComponent(k, out SenseSourceState<TouchSense> s))
            {
                s = new SenseSourceState<TouchSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
                v.AssignComponent(k, in s);
            }
            else
            {
                s = s.WithDirtyState(SenseSourceDirtyState.UnconditionallyDirty);
                v.ReplaceComponent(k, in s);
            }
            return true;
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out ITouchDirectionMap t)
        {
            if (v.GetComponent(k, out SingleLevelSenseDirectionMapData<TouchSense, TouchSense> m))
            {
                t = new SingleLevelTouchDirectionMap(m);
                return true;
            }

            t = default;
            return false;
        }
        
    }
}