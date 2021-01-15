using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Sensing
{
    public class SenseMappingTestContext : //IItemContext<ActorReference>,
                                           IItemContext<ItemReference>,
                                           IGridMapContext<ItemReference>,
                                           IGridMapContext<ActorReference>
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

            actorMap = new DefaultGridPositionContextBackend<ActorReference>()
                .WithDefaultMapLayer(TestMapLayers.Three);
        }

        int IGridMapConfiguration<ItemReference>.OffsetX => itemMap.OffsetX;

        int IGridMapConfiguration<ItemReference>.OffsetY => itemMap.OffsetY;

        int IGridMapConfiguration<ItemReference>.TileSizeX => itemMap.TileSizeX;

        int IGridMapConfiguration<ItemReference>.TileSizeY => itemMap.TileSizeY;

        int IGridMapConfiguration<ActorReference>.OffsetX =>  actorMap.OffsetX;

        int IGridMapConfiguration<ActorReference>.OffsetY =>  actorMap.OffsetY;

        int IGridMapConfiguration<ActorReference>.TileSizeX =>  actorMap.TileSizeX;

        int IGridMapConfiguration<ActorReference>.TileSizeY => actorMap.TileSizeY;

        ReadOnlyListWrapper<MapLayer> IGridMapContext<ActorReference>.GridLayers()
        {
            return actorMap.GridLayers();
        }

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

        bool IGridMapContext<ItemReference>.TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<ItemReference> data)
        {
            return itemMap.TryGetGridDataFor(layer, out data);
        }

        bool IGridMapContext<ActorReference>.TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<ActorReference> data)
        {
            return actorMap.TryGetGridDataFor(layer, out data);
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