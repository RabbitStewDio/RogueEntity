using RogueEntity.Core.Positioning.MapLayers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Positioning
{
    public class ItemPlacementServiceContext<TItemId> : IItemPlacementServiceContext<TItemId>
    {
        readonly Dictionary<byte, (IItemPlacementService<TItemId> placementService, IItemPlacementLocationService<TItemId> locatorService)> services;

        public ItemPlacementServiceContext()
        {
            this.services = new Dictionary<byte, (IItemPlacementService<TItemId>, IItemPlacementLocationService<TItemId>)>();
        }

        public ItemPlacementServiceContext<TItemId> WithLayer(MapLayer layer,
                                                              IItemPlacementService<TItemId> service,
                                                              IItemPlacementLocationService<TItemId> locator)
        {
            this.services[layer.LayerId] = (service ?? throw new ArgumentNullException(nameof(service)), 
                                            locator ?? throw new ArgumentNullException(nameof(locator)));
            service.ItemPositionChanged += OnItemPositionChanged;
            return this;
        }

        void OnItemPositionChanged(object sender, ItemPositionChangedEvent<TItemId> e)
        {
            
        }

        public bool TryGetItemPlacementService(byte layer, [MaybeNullWhen(false)] out IItemPlacementService<TItemId> service)
        {
            if (this.services.TryGetValue(layer, out var servicesForLayer))
            {
                service = servicesForLayer.placementService;
                return true;
            }

            service = default;
            return false;
        }
        
        public bool TryGetItemPlacementService(MapLayer layer, [MaybeNullWhen(false)] out IItemPlacementService<TItemId> service)
        {
            return TryGetItemPlacementService(layer.LayerId, out service);
        }
        
        public bool TryGetItemPlacementLocationService(MapLayer layer, [MaybeNullWhen(false)] out IItemPlacementLocationService<TItemId> service)
        {
            if (this.services.TryGetValue(layer.LayerId, out var servicesForLayer))
            {
                service = servicesForLayer.locatorService;
                return true;
            }

            service = default;
            return false;
        }
    }
}
