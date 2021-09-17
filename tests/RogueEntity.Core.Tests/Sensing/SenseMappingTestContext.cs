using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Tests.Fixtures;

namespace RogueEntity.Core.Tests.Sensing
{
    public class SenseMappingTestContext : IItemContext<ItemReference>,
                                           IGridMapContext<ItemReference>
    {
        readonly ItemContextBackend<ItemReference> itemBackend;
        readonly IGridMapContext<ItemReference> itemMap;
        readonly IGridMapContext<ActorReference> actorMap;

        public SenseMappingTestContext()
        {
            itemBackend = new ItemContextBackend<ItemReference>(new ItemReferenceMetaData());
            itemMap = new DefaultGridPositionContextBackend<ItemReference>()
                      .WithDefaultMapLayer(TestMapLayers.One)
                      .WithDefaultMapLayer(TestMapLayers.Two);

            ItemPlacementService = new GridItemPlacementService<ItemReference>(itemBackend.ItemResolver, itemMap);

            actorMap = new DefaultGridPositionContextBackend<ActorReference>()
                .WithDefaultMapLayer(TestMapLayers.Three);
        }

        public GridItemPlacementService<ItemReference> ItemPlacementService { get; }

        ReadOnlyListWrapper<MapLayer> IGridMapContext<ItemReference>.GridLayers()
        {
            return itemMap.GridLayers();
        }

        public bool TryGetItemGridDataFor(MapLayer layer, out IGridMapDataContext<ItemReference> data)
        {
            return itemMap.TryGetGridDataFor(layer, out data);
        }

        public bool TryGetActorGridDataFor(MapLayer layer, out IGridMapDataContext<ActorReference> data)
        {
            return actorMap.TryGetGridDataFor(layer, out data);
        }

        bool IGridMapContext<ItemReference>.TryGetGridDataFor(byte layerId, out IGridMapDataContext<ItemReference> data)
        {
            return itemMap.TryGetGridDataFor(layerId, out data);
        }

        bool IGridMapContext<ItemReference>.TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<ItemReference> data)
        {
            return itemMap.TryGetGridDataFor(layer, out data);
        }

        public IItemRegistryBackend<ItemReference> ItemRegistry
        {
            get { return itemBackend.ItemRegistry; }
        }

        public EntityRegistry<ItemReference> ItemEntityRegistry
        {
            get { return itemBackend.EntityRegistry; }
        }

        public IItemResolver<ItemReference> ItemResolver
        {
            get { return itemBackend.ItemResolver; }
        }
    }
}
