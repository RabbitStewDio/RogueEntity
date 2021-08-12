using RogueEntity.Core.Positioning.MapLayers;
using System;

namespace RogueEntity.Core.Positioning
{
    public interface IItemPlacementServiceContext<TItemId>
    {
        event EventHandler<ItemPositionChangedEvent<TItemId>> ItemPositionChanged; 

        bool TryGetItemPlacementService(MapLayer layer, out IItemPlacementService<TItemId> service);
        bool TryGetItemPlacementLocationService(MapLayer layer, out IItemPlacementLocationService<TItemId> service);
    }
}
