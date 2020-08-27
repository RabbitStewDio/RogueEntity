using EnTTSharp.Entities;
using GoRogue;
using GoRogue.SenseMapping;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Sensing.Vision
{
    public class LightSourceTrait<TGameContext, TItemId> : IItemComponentTrait<TGameContext, TItemId, LightSourceData>,
                                                               IReferenceItemTrait<TGameContext, TItemId>
        where TGameContext : IItemContext<TGameContext, TItemId> 
        where TItemId : IEntityKey
    {
        public bool Enabled { get; }
        public float Radius { get; }
        public float Strength { get; }

        public LightSourceTrait(float radius, float strength, bool enabled = true)
        {
            Enabled = enabled;
            Radius = radius;
            Strength = strength;
            Priority = 100;
            Id = "Core.Common.LightSource";
        }

        public string Id { get; }
        public int Priority { get; }

        protected LightSourceData CreateInitialValue()
        {
            var senseSource = new SmartSenseSource(SourceType.RIPPLE, Radius, DistanceCalculation.EUCLIDEAN, Strength);
            senseSource.MarkDirty();
            return new LightSourceData(senseSource, Enabled);
        }

        public void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            if (v.GetComponent(k, out LightSourceData l))
            {
                l.SenseSource.Reset(SourceType.RIPPLE, Radius, DistanceCalculation.EUCLIDEAN, Strength);
            }
            else
            {
                v.AssignOrReplace(k, CreateInitialValue());
            }
        }

        public void Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            v.AssignOrReplace(k, CreateInitialValue());
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
            changedK = k;
            v.AssignOrReplace(k, t);
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> entityRegistry, TGameContext context, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }
    }
}