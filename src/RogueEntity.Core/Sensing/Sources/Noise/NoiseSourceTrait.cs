using System;
using EnTTSharp.Entities;
using GoRogue;
using GoRogue.SenseMapping;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public class NoiseSourceTrait<TGameContext, TItemId>: IReferenceItemTrait<TGameContext, TItemId>,
                                                          IItemComponentTrait<TGameContext, TItemId, NoiseSoundClip>
        where TItemId : IEntityKey
        where TGameContext: INoisePhysicsContext
    {
        public string Id => "Core.Item.NoiseSource";
        public int Priority => 100;

        readonly float intensityHint;

        public NoiseSourceTrait(float intensityHint = 10)
        {
            this.intensityHint = Math.Max(intensityHint, 1);
        }

        public void Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            var radius = context.NoiseSignalRadiusForIntensity(intensityHint);
            var senseSource = new SmartSenseSource(SourceType.RIPPLE_VERY_LOOSE, radius, DistanceCalculation.EUCLIDEAN, intensityHint);
            v.AssignComponent(k, new NoiseSourceState(senseSource));
        }

        public void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out NoiseSoundClip t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in NoiseSoundClip t, out TItemId changedK)
        {
            v.AssignOrReplace(k, in t);
            changedK = k;
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TItemId changedK)
        {
            v.RemoveComponent<NoiseSoundClip>(k);
            changedK = k;
            return true;
        }
    }
}