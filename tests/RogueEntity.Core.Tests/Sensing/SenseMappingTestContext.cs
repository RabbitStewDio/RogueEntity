using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Sensing
{
    public class SenseMappingTestContext : //IItemContext<SenseMappingTestContext, ActorReference>,
                                           IItemContext<SenseMappingTestContext, ItemReference>,
                                           IGridMapContext<ItemReference>,
                                           IGridMapContext<ActorReference>
    {
        readonly ItemContextBackend<SenseMappingTestContext, ActorReference> actorBackend;
        readonly ItemContextBackend<SenseMappingTestContext, ItemReference> itemBackend;
        readonly IGridMapContext<ItemReference> itemMap;
        readonly IGridMapContext<ActorReference> actorMap;

        public SenseMappingTestContext()
        {
            actorBackend = new ItemContextBackend<SenseMappingTestContext, ActorReference>(new ActorReferenceMetaData());
            itemBackend = new ItemContextBackend<SenseMappingTestContext, ItemReference>(new ItemReferenceMetaData());
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

        public IItemRegistryBackend<SenseMappingTestContext, ItemReference> ItemRegistry
        {
            get { return itemBackend.ItemRegistry; }
        }

        public EntityRegistry<ItemReference> ItemEntityRegistry
        {
            get { return itemBackend.EntityRegistry; }
        }

        public IItemResolver<SenseMappingTestContext, ItemReference> ItemResolver
        {
            get { return itemBackend.ItemResolver; }
        }

        // public IItemRegistryBackend<SenseMappingTestContext, ActorReference> ActorRegistry
        // {
        //     get { return actorBackend.ItemRegistry; }
        // }
        //
        // public EntityRegistry<ActorReference> ActorEntityRegistry
        // {
        //     get { return actorBackend.EntityRegistry; }
        // }
        //
        // public IItemResolver<SenseMappingTestContext, ActorReference> ActorResolver
        // {
        //     get { return actorBackend.ItemResolver; }
        // }

        //IItemResolver<SenseMappingTestContext, ActorReference> IItemContext<SenseMappingTestContext, ActorReference>.ItemResolver => ActorResolver;
    }
}