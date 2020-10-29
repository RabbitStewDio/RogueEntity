using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    public class HeatSourceTrait<TGameContext, TItemId> : IReferenceItemTrait<TGameContext, TItemId>,
                                                          IItemComponentTrait<TGameContext, TItemId, Temperature>,
                                                          IItemComponentTrait<TGameContext, TItemId, HeatSourceDefinition>
        where TItemId : IEntityKey
    {
        public string Id => "Core.Item.Temperature";
        public int Priority => 100;

        readonly IHeatPhysicsConfiguration physicsConfiguration;
        readonly Optional<Temperature> baseTemperature;

        public HeatSourceTrait([NotNull] IHeatPhysicsConfiguration physicsConfiguration, Optional<Temperature> baseTemperature = default)
        {
            this.physicsConfiguration = physicsConfiguration ?? throw new ArgumentNullException(nameof(physicsConfiguration));
            this.baseTemperature = baseTemperature;
        }

        public void Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            if (baseTemperature.TryGetValue(out var value))
            {
                v.AssignComponent(k, new HeatSourceDefinition(new SenseSourceDefinition(physicsConfiguration.HeatPhysics.DistanceMeasurement, value.ToKelvin()), true));
                v.AssignComponent(k, new SenseSourceState<TemperatureSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid));
            }
        }

        public void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            if (!v.GetComponent(k, out HeatSourceDefinition _))
            {
                return;
            }

            if (!v.GetComponent(k, out SenseSourceState<TemperatureSense> s))
            {
                s = new SenseSourceState<TemperatureSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
                v.AssignComponent(k, in s);
            }
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

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out HeatSourceDefinition t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in Temperature t, out TItemId changedK)
        {
            if (TryQuery(v, context, k, out HeatSourceDefinition h))
            {
                return TryUpdate(v, context, k, h.WithSenseSource(h.SenseDefinition.WithIntensity(t.ToKelvin())), out changedK);
            }

            var val = new HeatSourceDefinition(new SenseSourceDefinition(physicsConfiguration.HeatPhysics.DistanceMeasurement, t.ToKelvin()), true);
            return TryUpdate(v, context, k, in val, out changedK);
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in HeatSourceDefinition t, out TItemId changedK)
        {
            v.AssignOrReplace(k, t);
            
            if (!v.GetComponent(k, out SenseSourceState<TemperatureSense> s))
            {
                s = new SenseSourceState<TemperatureSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
                v.AssignComponent(k, in s);
            }
            else
            {
                s = s.WithDirtyState(SenseSourceDirtyState.UnconditionallyDirty);
                v.AssignComponent(k, in s);
            }

            changedK = k;
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TItemId changedK)
        {
            v.RemoveComponent<HeatSourceDefinition>(k);
            v.RemoveComponent<SenseSourceState<TemperatureSense>>(k);
            changedK = k;
            return true;
        }
    }
}