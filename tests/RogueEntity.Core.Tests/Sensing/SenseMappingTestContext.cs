using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Sensing
{
    public class SenseMappingTestContext : IItemContext<SenseMappingTestContext, ActorReference>,
                                           IItemContext<SenseMappingTestContext, ItemReference>,
                                           IGridMapContext<SenseMappingTestContext, ItemReference>,
                                           IGridMapContext<SenseMappingTestContext, ActorReference>
    {
        readonly ItemContextBackend<SenseMappingTestContext, ActorReference> actorBackend;
        readonly ItemContextBackend<SenseMappingTestContext, ItemReference> itemBackend;
        readonly IGridMapContext<SenseMappingTestContext, ItemReference> itemMap;
        readonly IGridMapContext<SenseMappingTestContext, ActorReference> actorMap;

        public SenseMappingTestContext()
        {
            actorBackend = new ItemContextBackend<SenseMappingTestContext, ActorReference>(new ActorReferenceMetaData());
            itemBackend = new ItemContextBackend<SenseMappingTestContext, ItemReference>(new ItemReferenceMetaData());
            itemMap = new DefaultGridPositionContextBackend<SenseMappingTestContext, ItemReference>()
                .WithDefaultMapLayer(TestMapLayers.One)
                .WithDefaultMapLayer(TestMapLayers.Two);

            actorMap = new DefaultGridPositionContextBackend<SenseMappingTestContext, ActorReference>()
                .WithDefaultMapLayer(TestMapLayers.Three);
        }

        ReadOnlyListWrapper<MapLayer> IGridMapContext<SenseMappingTestContext, ActorReference>.GridLayers()
        {
            return actorMap.GridLayers();
        }
        
        public bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<SenseMappingTestContext, ActorReference> data)
        {
            return actorMap.TryGetGridDataFor(layer, out data);
        }

        public bool TryGetGridRawDataFor(MapLayer layer, out IGridMapRawDataContext<ActorReference> data)
        {
            return actorMap.TryGetGridRawDataFor(layer, out data);
        }

        ReadOnlyListWrapper<MapLayer> IGridMapContext<SenseMappingTestContext, ItemReference>.GridLayers()
        {
            return itemMap.GridLayers();
        }

        public bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<SenseMappingTestContext, ItemReference> data)
        {
            return itemMap.TryGetGridDataFor(layer, out data);
        }

        public bool TryGetGridRawDataFor(MapLayer layer, out IGridMapRawDataContext<ItemReference> data)
        {
            return itemMap.TryGetGridRawDataFor(layer, out data);
        }

        public ItemRegistry<SenseMappingTestContext, ItemReference> ItemRegistry
        {
            get { return itemBackend.ItemRegistry; }
        }

        public EntityRegistry<ItemReference> EntityRegistry
        {
            get { return itemBackend.EntityRegistry; }
        }

        public IItemResolver<SenseMappingTestContext, ItemReference> ItemResolver
        {
            get { return itemBackend.ItemResolver; }
        }

        public ItemRegistry<SenseMappingTestContext, ActorReference> ActorRegistry
        {
            get { return actorBackend.ItemRegistry; }
        }

        public EntityRegistry<ActorReference> ActorEntityRegistry
        {
            get { return actorBackend.EntityRegistry; }
        }

        public IItemResolver<SenseMappingTestContext, ActorReference> ActorResolver
        {
            get { return actorBackend.ItemResolver; }
        }

        IItemResolver<SenseMappingTestContext, ActorReference> IItemContext<SenseMappingTestContext, ActorReference>.ItemResolver => ActorResolver;
    }
}