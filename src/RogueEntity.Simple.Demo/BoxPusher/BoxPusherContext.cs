namespace RogueEntity.Simple.Demo.BoxPusher
{
    public class BoxPusherContext
/*        : IItemContextBackend<BoxPusherContext, ItemReference>, 
                                   IItemContextBackend<BoxPusherContext, ActorReference>,
                                   IGridMapContext<ItemReference>,
                                   IGridMapContext<ActorReference>*/
    {
        /*
        ItemContextBackend<BoxPusherContext, ItemReference> itemContext;
        ItemContextBackend<BoxPusherContext, ActorReference> actorContext;

        DefaultGridPositionContextBackend<BoxPusherContext, ItemReference> itemMap;
        DefaultGridPositionContextBackend<BoxPusherContext, ActorReference> actorMap;
        
        public BoxPusherContext(int tileWidth, int tileHeight)
        {
            itemContext = new ItemContextBackend<BoxPusherContext, ItemReference>(new ItemReferenceMetaData());
            actorContext = new ItemContextBackend<BoxPusherContext, ActorReference>(new ActorReferenceMetaData());
            
            itemMap = new DefaultGridPositionContextBackend<BoxPusherContext, ItemReference>()
                .WithMapLayer(BoxPusherMapLayers.Floor, new DefaultGridMapDataContext<ItemReference>(BoxPusherMapLayers.Floor, tileWidth, tileHeight))
                .WithMapLayer(BoxPusherMapLayers.Items, new DefaultGridMapDataContext<ItemReference>(BoxPusherMapLayers.Items, tileWidth, tileHeight));
            actorMap = new DefaultGridPositionContextBackend<BoxPusherContext, ActorReference>()
                .WithMapLayer(BoxPusherMapLayers.Actors, new DefaultGridMapDataContext<ActorReference>(BoxPusherMapLayers.Actors, tileWidth, tileHeight));
        }

        IItemResolver<BoxPusherContext, ItemReference> IItemContext<BoxPusherContext, ItemReference>.ItemResolver => itemContext.ItemResolver;
        IItemResolver<BoxPusherContext, ActorReference> IItemContext<BoxPusherContext, ActorReference>.ItemResolver => actorContext.ItemResolver;

        ReadOnlyListWrapper<MapLayer> IGridMapContext<ItemReference>.GridLayers() => itemMap.GridLayers();
        ReadOnlyListWrapper<MapLayer> IGridMapContext<ActorReference>.GridLayers() => actorMap.GridLayers();

        public bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<ItemReference> data)
        {
            return itemMap.TryGetGridDataFor(layer, out data);
        }

        public bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<ActorReference> data)
        {
            return actorMap.TryGetGridDataFor(layer, out data);
        }

        public int OffsetX
        {
            get { return itemMap.OffsetX; }
        }

        public int OffsetY
        {
            get { return itemMap.OffsetY; }
        }

        public int TileSizeX
        {
            get { return itemMap.TileSizeX; }
        }

        public int TileSizeY
        {
            get { return itemMap.TileSizeY; }
        }

        public IItemRegistryBackend<BoxPusherContext, ActorReference> ActorRegistry
        {
            get { return actorContext.ItemRegistry; }
        }

        public EntityRegistry<ActorReference> ActorEntities
        {
            get { return actorContext.EntityRegistry; }
        }

        EntityRegistry<ItemReference> IItemContextBackend<BoxPusherContext, ItemReference>.EntityRegistry
        {
            get { return ItemEntities; }
        }

        EntityRegistry<ActorReference> IItemContextBackend<BoxPusherContext, ActorReference>.EntityRegistry
        {
            get { return ActorEntities; }
        }

        IItemRegistryBackend<BoxPusherContext, ActorReference> IItemContextBackend<BoxPusherContext, ActorReference>.ItemRegistry
        {
            get { return ActorRegistry; }
        }

        public IItemResolver<BoxPusherContext, ActorReference> ActorResolver
        {
            get { return actorContext.ItemResolver; }
        }
        
        public IItemRegistryBackend<BoxPusherContext, ItemReference> ItemRegistry
        {
            get { return itemContext.ItemRegistry; }
        }

        public EntityRegistry<ItemReference> ItemEntities
        {
            get { return itemContext.EntityRegistry; }
        }

        public IItemResolver<BoxPusherContext, ItemReference> ItemResolver
        {
            get { return itemContext.ItemResolver; }
        }
        */
    }
}