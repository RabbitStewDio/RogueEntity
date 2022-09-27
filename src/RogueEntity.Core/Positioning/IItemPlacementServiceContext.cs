using RogueEntity.Core.Positioning.MapLayers;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Positioning
{
    public interface IItemPlacementServiceContext<TItemId>
    {
        bool TryGetItemPlacementService(MapLayer layer, [MaybeNullWhen(false)] out IItemPlacementService<TItemId> service);
        bool TryGetItemPlacementService(byte layerId, [MaybeNullWhen(false)] out IItemPlacementService<TItemId> service);
        
        bool TryGetItemPlacementLocationService(MapLayer layer, [MaybeNullWhen(false)] out IItemPlacementLocationService<TItemId> service);
    }
}
