using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.Sensing
{
    public class SenseMappingTestContext : IItemContext<ItemReference>,
                                           IMapContext<ItemReference>
    {
        readonly ItemContextBackend<ItemReference> itemBackend;
        readonly IMapContext<ItemReference> itemMap;
        readonly IMapContext<ActorReference> actorMap;

        public SenseMappingTestContext()
        {
            itemBackend = new ItemContextBackend<ItemReference>(new ItemReferenceMetaData());
            itemMap = new DefaultMapContext<ItemReference>(DynamicDataViewConfiguration.Default16X16)
                      .WithBasicGridMapLayer(TestMapLayers.One)
                      .WithBasicGridMapLayer(TestMapLayers.Two);

            ItemPlacementService = new ItemPlacementService<ItemReference>(itemBackend.ItemResolver, itemMap);

            actorMap = new DefaultMapContext<ActorReference>(DynamicDataViewConfiguration.Default16X16)
                .WithBasicGridMapLayer(TestMapLayers.Three);
        }

        public ItemPlacementService<ItemReference> ItemPlacementService { get; }

        ReadOnlyListWrapper<MapLayer> IMapContext<ItemReference>.Layers()
        {
            return itemMap.Layers();
        }

        public bool TryGetItemGridDataFor(MapLayer layer, out IMapDataContext<ItemReference> data)
        {
            return itemMap.TryGetMapDataFor(layer, out data);
        }

        public bool TryGetActorGridDataFor(MapLayer layer, out IMapDataContext<ActorReference> data)
        {
            return actorMap.TryGetMapDataFor(layer, out data);
        }

        bool IMapContext<ItemReference>.TryGetMapDataFor(byte layerId, out IMapDataContext<ItemReference> data)
        {
            return itemMap.TryGetMapDataFor(layerId, out data);
        }

        bool IMapContext<ItemReference>.TryGetMapDataFor(MapLayer layer, out IMapDataContext<ItemReference> data)
        {
            return itemMap.TryGetMapDataFor(layer, out data);
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
