using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Sources.Smell
{
    public class SmellSourceTrait<TGameContext, TItemId>: IReferenceItemTrait<TGameContext, TItemId>,
                                                          IItemComponentTrait<TGameContext, TItemId, SmellSource>
        where TItemId : IEntityKey
    {
        public string Id => "Core.Item.SmellSource";
        public int Priority => 100;

        readonly ISmellPhysicsConfiguration physics;

        public SmellSourceTrait([NotNull] ISmellPhysicsConfiguration physics)
        {
            this.physics = physics ?? throw new ArgumentNullException(nameof(physics));
        }

        public void Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        public void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out SmellSource t)
        {
            if (v.GetComponent(k, out SmellSourceDefinition d))
            {
                t = d.Smell;
                return true;
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in SmellSource t, out TItemId changedK)
        {
            v.AssignOrReplace(k, new SmellSourceDefinition(new SenseSourceDefinition(physics.SmellPhysics.DistanceMeasurement, t.Intensity), t, true));
            changedK = k;
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TItemId changedK)
        {
            v.RemoveComponent<SmellSourceDefinition>(k);
            changedK = k;
            return true;
        }
    }
}