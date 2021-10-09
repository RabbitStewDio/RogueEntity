using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning
{
    public interface IItemPlacementServiceContext<TItemId>
    {
        bool TryGetItemPlacementService(MapLayer layer, out IItemPlacementService<TItemId> service);
        bool TryGetItemPlacementService(byte layerId, out IItemPlacementService<TItemId> service);
        
        bool TryGetItemPlacementLocationService(MapLayer layer, out IItemPlacementLocationService<TItemId> service);
    }
}
