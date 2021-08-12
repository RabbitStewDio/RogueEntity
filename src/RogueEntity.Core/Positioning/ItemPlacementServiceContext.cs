using JetBrains.Annotations;
using RogueEntity.Core.Positioning.MapLayers;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning
{
    public class ItemPlacementServiceContext<TItemId> : IItemPlacementServiceContext<TItemId>
    {
        public event EventHandler<ItemPositionChangedEvent<TItemId>> ItemPositionChanged; 

        readonly Dictionary<byte, (IItemPlacementService<TItemId> placementService, IItemPlacementLocationService<TItemId> locatorService)> services;

        public ItemPlacementServiceContext()
        {
            this.services = new Dictionary<byte, (IItemPlacementService<TItemId>, IItemPlacementLocationService<TItemId>)>();
        }

        public ItemPlacementServiceContext<TItemId> WithLayer(MapLayer layer,
                                                              [NotNull] IItemPlacementService<TItemId> service,
                                                              [NotNull] IItemPlacementLocationService<TItemId> locator)
        {
            this.services[layer.LayerId] = (service ?? throw new ArgumentNullException(nameof(service)), 
                                            locator ?? throw new ArgumentNullException(nameof(locator)));
            service.ItemPositionChanged += OnItemPositionChanged;
            return this;
        }

        void OnItemPositionChanged(object sender, ItemPositionChangedEvent<TItemId> e)
        {
            
        }

        public bool TryGetItemPlacementService(MapLayer layer, out IItemPlacementService<TItemId> service)
        {
            if (this.services.TryGetValue(layer.LayerId, out var servicesForLayer))
            {
                service = servicesForLayer.placementService;
                return true;
            }

            service = default;
            return false;
        }
        
        public bool TryGetItemPlacementLocationService(MapLayer layer, out IItemPlacementLocationService<TItemId> service)
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
