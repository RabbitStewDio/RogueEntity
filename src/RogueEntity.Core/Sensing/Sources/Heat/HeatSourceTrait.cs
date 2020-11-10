using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    public class HeatSourceTrait<TGameContext, TItemId> : SenseSourceTraitBase<TGameContext, TItemId, TemperatureSense, HeatSourceDefinition>,
                                                          IItemComponentTrait<TGameContext, TItemId, Temperature>
        where TItemId : IEntityKey
    {
        public override string Id => "Core.Item.Temperature";
        public override int Priority => 100;

        readonly IHeatPhysicsConfiguration physicsConfiguration;
        readonly Optional<Temperature> baseTemperature;

        public HeatSourceTrait([NotNull] IHeatPhysicsConfiguration physicsConfiguration, Optional<Temperature> baseTemperature = default)
        {
            this.physicsConfiguration = physicsConfiguration ?? throw new ArgumentNullException(nameof(physicsConfiguration));
            this.baseTemperature = baseTemperature;
        }
        
        protected override bool TryGetInitialValue(out HeatSourceDefinition senseDefinition)
        {
            if (baseTemperature.TryGetValue(out var value))
            {
                senseDefinition = new HeatSourceDefinition(new SenseSourceDefinition(physicsConfiguration.HeatPhysics.DistanceMeasurement,
                                                                                     physicsConfiguration.HeatPhysics.AdjacencyRule,
                                                                                     value.ToKelvin()), true);                
                return true;
            }

            senseDefinition = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out Temperature t)
        {
            if (TryQuery(v, context, k, out HeatSourceDefinition d))
            {
                t = Temperature.FromKelvin(d.SenseDefinition.Intensity);
                return true;
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in Temperature t, out TItemId changedK)
        {
            if (TryQuery(v, context, k, out HeatSourceDefinition h))
            {
                return TryUpdate(v, context, k, h.WithSenseSource(h.SenseDefinition.WithIntensity(t.ToKelvin())), out changedK);
            }

            var val = new HeatSourceDefinition(new SenseSourceDefinition(physicsConfiguration.HeatPhysics.DistanceMeasurement,
                                                                         physicsConfiguration.HeatPhysics.AdjacencyRule,
                                                                         t.ToKelvin()), true);
            return TryUpdate(v, context, k, in val, out changedK);
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return SenseSourceModules.GetSourceRole<TemperatureSense>().Instantiate<TItemId>();
        }
    }
}