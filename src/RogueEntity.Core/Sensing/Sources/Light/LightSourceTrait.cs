using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    public class LightSourceTrait<TGameContext, TItemId> : IItemComponentTrait<TGameContext, TItemId, LightSourceDefinition>,
                                                           IReferenceItemTrait<TGameContext, TItemId>
        where TGameContext : IItemContext<TGameContext, TItemId>
        where TItemId : IEntityKey
    {
        readonly ILightPhysicsConfiguration lightPhysics;
        public float Hue { get; }
        public float Saturation { get; }
        public float Intensity { get; }
        public bool Enabled { get; }

        public LightSourceTrait(ILightPhysicsConfiguration lightPhysics, float hue, float saturation, float intensity, bool enabled)
        {
            this.lightPhysics = lightPhysics;
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
        public LightSourceTrait(ILightPhysicsConfiguration lightPhysics, 
                                float intensity, 
                                bool enabled = true): this(lightPhysics, 0, 0, intensity, enabled)
        {
        }

        public string Id => "Core.Common.LightSource"; 
        public int Priority => 100;

        public void Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            var l = new LightSourceDefinition(new SenseSourceDefinition(lightPhysics.LightPhysics.DistanceMeasurement, Intensity), Hue, Saturation, Enabled);
            var s = new SenseSourceState<VisionSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);

            v.AssignOrReplace(k, l);
            v.AssignComponent(k, s);
        }

        public void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            if (!v.GetComponent(k, out LightSourceDefinition l))
            {
                return;
            }

            if (!v.GetComponent(k, out SenseSourceState<VisionSense> s))
            {
                s = new SenseSourceState<VisionSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
                v.AssignComponent(k, in s);
            }
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out LightSourceDefinition t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in LightSourceDefinition t, out TItemId changedK)
        {
            changedK = k;
            if (v.GetComponent(k, out LightSourceDefinition existing))
            {
                if (existing == t)
                {
                    return true;
                }
            }
            
            v.AssignComponent(k, t);
            if (!v.GetComponent(k, out SenseSourceState<VisionSense> s))
            {
                s = new SenseSourceState<VisionSense>(Optional.Empty<SenseSourceData>(), SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
                v.AssignComponent(k, in s);
            }
            else
            {
                s = s.WithDirtyState(SenseSourceDirtyState.UnconditionallyDirty);
                v.AssignComponent(k, in s);
            }
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }
    }
}