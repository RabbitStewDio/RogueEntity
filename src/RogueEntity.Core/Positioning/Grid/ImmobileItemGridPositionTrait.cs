using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Grid
{
    public class ImmobileItemGridPositionTrait<TGameContext, TItemId> : IBulkItemTrait<TGameContext, TItemId>,
                                                                IReferenceItemTrait<TGameContext, TItemId>,
                                                                IItemComponentTrait<TGameContext, TItemId, EntityGridPosition>,
                                                                IItemComponentTrait<TGameContext, TItemId, MapLayerPreference>
        where TGameContext : IGridMapContext<TGameContext, TItemId>, IItemContext<TGameContext, TItemId>
        where TItemId : IEntityKey
    {
        readonly MapLayerPreference preferredLayer;

        public ImmobileItemGridPositionTrait(MapLayer preferredLayer)
        {
            this.preferredLayer = new MapLayerPreference(preferredLayer);
            Id = "Item.Generic.Positional";
            Priority = 100;
        }

        public string Id { get; }
        public int Priority { get; }

        void IReferenceItemTrait<TGameContext, TItemId>.Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        void IReferenceItemTrait<TGameContext, TItemId>.Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out EntityGridPosition t)
        {
            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in EntityGridPosition t, out TItemId changedK)
        {
            changedK = k;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out MapLayerPreference t)
        {
            t = preferredLayer;
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in MapLayerPreference t, out TItemId changedK)
        {
            changedK = k;
            return false;
        }

        bool IItemComponentTrait<TGameContext, TItemId, EntityGridPosition>.TryRemove(IEntityViewControl<TItemId> entityRegistry, TGameContext context, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }

        bool IItemComponentTrait<TGameContext, TItemId, MapLayerPreference>.TryRemove(IEntityViewControl<TItemId> entityRegistry, TGameContext context, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }

        public TItemId Initialize(TGameContext context, IItemDeclaration item, TItemId reference)
        {
            return reference;
        }
    }
}