using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Sources.Smell
{
    public class SmellSourceTrait<TGameContext, TItemId>: SenseSourceTraitBase<TGameContext, TItemId, SmellSense, SmellSourceDefinition>,
                                                          IItemComponentTrait<TGameContext, TItemId, SmellSource>
        where TItemId : IEntityKey
    {
        public override string Id => "Core.Item.SmellSource";
        public override int Priority => 100;

        readonly ISmellPhysicsConfiguration physics;

        public SmellSourceTrait([NotNull] ISmellPhysicsConfiguration physics)
        {
            this.physics = physics ?? throw new ArgumentNullException(nameof(physics));
        }

        protected override bool TryGetInitialValue(out SmellSourceDefinition senseDefinition)
        {
            senseDefinition = default;
            return false;
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
            return TryUpdate(v, context, k, 
                             new SmellSourceDefinition(new SenseSourceDefinition(physics.SmellPhysics.DistanceMeasurement, 
                                                                                     physics.SmellPhysics.AdjacencyRule, 
                                                                                     t.Intensity), t, true), out changedK);
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return SenseSourceModules.GetSourceRole<SmellSense>().Instantiate<TItemId>();
        }
    }
}