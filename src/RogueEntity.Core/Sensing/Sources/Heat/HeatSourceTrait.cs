using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    public class HeatSourceTrait<TGameContext, TItemId> : IReferenceItemTrait<TGameContext, TItemId>,
                                                          IItemComponentTrait<TGameContext, TItemId, Temperature>
        where TItemId : IEntityKey
        where TGameContext: IHeatPhysicsConfiguration
    {
        public string Id => "Core.Item.Temperature";
        public int Priority => 100;

        readonly Optional<Temperature> baseTemperature;

        public HeatSourceTrait(Optional<Temperature> baseTemperature = default)
        {
            this.baseTemperature = baseTemperature;
        }

        public void Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            if (baseTemperature.TryGetValue(out var value))
            {
                v.AssignComponent(k, value);
            }
        }

        public void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            if (baseTemperature.TryGetValue(out var value))
            {
                v.AssignComponent(k, value);
            }
            else
            {
                v.RemoveComponent<Temperature>(k);
            }
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out Temperature t)
        {
            if (v.GetComponent(k, out t))
            {
                return true;
            }

            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in Temperature t, out TItemId changedK)
        {
            v.AssignOrReplace(k, t);
            changedK = k;
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TItemId changedK)
        {
            v.RemoveComponent<Temperature>(k);
            changedK = k;
            return true;
        }
    }
}