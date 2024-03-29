using EnTTSharp;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Touch;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Receptors.Touch
{
    public sealed class TouchSenseTrait<TActorId> : SenseReceptorTraitBase<TActorId, TouchSense, TouchSense>,
                                                    IItemComponentInformationTrait<TActorId, ITouchDirectionMap>,
                                                    IItemComponentTrait<TActorId, TouchSourceDefinition>
        where TActorId : struct, IEntityKey
    {
        readonly ITouchReceptorPhysicsConfiguration touchPhysics;

        public TouchSenseTrait(ITouchReceptorPhysicsConfiguration touchPhysics, bool active = true) : base(touchPhysics.TouchPhysics, GetStandardIntensity(touchPhysics), active)
        {
            this.touchPhysics = touchPhysics;
        }

        static float GetStandardIntensity(ITouchReceptorPhysicsConfiguration p) => p.TouchPhysics.DistanceMeasurement.MaximumStepDistance();

        public override ItemTraitId Id => "Core.Sense.Receptor.Touch";
        public override int Priority => 100;

        public override void Initialize(IEntityViewControl<TActorId> v, TActorId k, IItemDeclaration item)
        {
            base.Initialize(v, k, item);

            v.AssignComponent(k, new TouchSourceDefinition(new SenseSourceDefinition(touchPhysics.TouchPhysics.DistanceMeasurement,
                                                                                     touchPhysics.TouchPhysics.AdjacencyRule,
                                                                                     Intensity), true));
            v.AssignComponent(k, new SenseSourceState<TouchSense>(Optional.Empty(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid));
        }

        public override void Apply(IEntityViewControl<TActorId> v, TActorId k, IItemDeclaration item)
        {
            if (v.HasComponent<TouchSourceDefinition>(k) && !v.HasComponent<SenseSourceState<TouchSense>>(k))
            {
                v.AssignComponent(k, new SenseSourceState<TouchSense>(Optional.Empty(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid));
            }
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TActorId k, out TouchSourceDefinition t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TActorId k, in TouchSourceDefinition t, out TActorId changedK)
        {
            v.AssignOrReplace(k, in t);
            changedK = k;

            if (!v.GetComponent<SenseSourceState<TouchSense>>(k, out var s))
            {
                s = new SenseSourceState<TouchSense>(Optional.Empty(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
                v.AssignComponent(k, in s);
            }
            else
            {
                s = s.WithDirtyState(SenseSourceDirtyState.UnconditionallyDirty);
                v.ReplaceComponent(k, in s);
            }

            return true;
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TActorId k, [MaybeNullWhen(false)] out ITouchDirectionMap t)
        {
            if (v.GetComponent(k, out SingleLevelSenseDirectionMapData<TouchSense, TouchSense> m))
            {
                t = new SingleLevelTouchDirectionMap(m);
                return true;
            }

            t = default;
            return false;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return TouchSenseModule.SenseReceptorActorRole.Instantiate<TActorId>();
            yield return SenseSourceModules.GetSourceRole<TouchSense>().Instantiate<TActorId>();
        }
    }
}
