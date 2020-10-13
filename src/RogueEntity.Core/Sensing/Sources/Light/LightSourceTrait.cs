using EnTTSharp.Entities;
using GoRogue;
using GoRogue.SenseMapping;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    public class LightSourceTrait<TGameContext, TItemId> : IItemComponentTrait<TGameContext, TItemId, LightSourceData>,
                                                           IReferenceItemTrait<TGameContext, TItemId>
        where TGameContext : IItemContext<TGameContext, TItemId>, ILightPhysicsConfiguration
        where TItemId : IEntityKey
    {
        public float Hue { get; }
        public float Saturation { get; }
        public float Intensity { get; }
        public bool Enabled { get; }

        public LightSourceTrait(float hue, float saturation, float intensity, bool enabled)
        {
            Hue = hue.Clamp(0, 1);
            Saturation = saturation;
            Intensity = intensity;
            Enabled = enabled;
        }

        /// <summary>
        ///   Creates a white light.
        /// </summary>
        /// <param name="intensity"></param>
        /// <param name="enabled"></param>
        public LightSourceTrait(float intensity, bool enabled = true)
        {
            Hue = 0;
            Saturation = 0;
            Enabled = enabled;
            Intensity = intensity;
        }

        public string Id => "Core.Common.LightSource"; 
        public int Priority => 100;

        public void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            if (!v.GetComponent(k, out LightSourceData l))
            {
                return;
            }

            if (v.GetComponent(k, out LightSourceState s))
            {
                var radius = context.LightSignalRadiusForIntensity(l.Intensity);
                s.SenseSource.Reset(SourceType.RIPPLE, radius, DistanceCalculation.EUCLIDEAN, l.Intensity);
            }
        }

        public void Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            var radius = context.LightSignalRadiusForIntensity(Intensity);
            var l = new LightSourceData(Hue, Saturation, Intensity, Enabled);
            var source = new SmartSenseSource(SourceType.RIPPLE, radius, DistanceCalculation.EUCLIDEAN, l.Intensity);
            source.Enabled = l.Enabled;
            source.MarkDirty();

            v.AssignOrReplace(k, l);
            v.AssignComponent(k, new LightSourceState(source));
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out LightSourceData t)
        {
            if (v.GetComponent(k, out LightSourceData l))
            {
                t = l;
                return true;
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in LightSourceData t, out TItemId changedK)
        {
            var radius = context.LightSignalRadiusForIntensity(t.Intensity);
            
            changedK = k;
            v.AssignOrReplace(k, t);
            if (v.GetComponent(k, out LightSourceState s))
            {
                s.SenseSource.Reset(SourceType.RIPPLE, radius, DistanceCalculation.EUCLIDEAN, t.Intensity);
            }
            else
            {
                var source = new SmartSenseSource(SourceType.RIPPLE, radius, DistanceCalculation.EUCLIDEAN, t.Intensity);
                source.Enabled = t.Enabled;
                source.MarkDirty();
                v.AssignComponent(k, new LightSourceState(source));
            }
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TItemId changedItem)
        {
            v.RemoveComponent<LightSourceData>(k);
            v.RemoveComponent<LightSourceState>(k);
            changedItem = k;
            return true;
        }
    }
}