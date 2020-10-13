using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Simple.BoxPusher
{
    public class BoxPusherContext: IItemContext<BoxPusherContext, ItemReference>, 
                                   IItemContext<BoxPusherContext, ActorReference>,
                                   IGridMapContext<BoxPusherContext, ItemReference>,
                                   IGridMapContext<BoxPusherContext, ActorReference>,
                                   IMapBoundsContext
    {
        ItemContextBackend<BoxPusherContext, ItemReference> itemContext;
        ItemContextBackend<BoxPusherContext, ActorReference> actorContext;

        DefaultGridPositionContextBackend<BoxPusherContext, ItemReference> itemMap;
        DefaultGridPositionContextBackend<BoxPusherContext, ActorReference> actorMap;

        public BoxPusherContext(int width, int height)
        {
            itemContext = new ItemContextBackend<BoxPusherContext, ItemReference>(new ItemReferenceMetaData());
            actorContext = new ItemContextBackend<BoxPusherContext, ActorReference>(new ActorReferenceMetaData());
            
            itemMap = new DefaultGridPositionContextBackend<BoxPusherContext, ItemReference>()
                .WithMapLayer(BoxPusherMapLayers.Floor, new OnDemandGridMapDataContext<BoxPusherContext, ItemReference>(BoxPusherMapLayers.Floor, width, height))
                .WithMapLayer(BoxPusherMapLayers.Items, new OnDemandGridMapDataContext<BoxPusherContext, ItemReference>(BoxPusherMapLayers.Items, width, height));
            actorMap = new DefaultGridPositionContextBackend<BoxPusherContext, ActorReference>()
                .WithMapLayer(BoxPusherMapLayers.Actors, new OnDemandGridMapDataContext<BoxPusherContext, ActorReference>(BoxPusherMapLayers.Actors, width, height));
            
            MapExtent = new MapBoundary(width, height, 5);
        }

        public MapBoundary MapExtent { get; }

        IItemResolver<BoxPusherContext, ItemReference> IItemContext<BoxPusherContext, ItemReference>.ItemResolver => itemContext.ItemResolver;
        IItemResolver<BoxPusherContext, ActorReference> IItemContext<BoxPusherContext, ActorReference>.ItemResolver => actorContext.ItemResolver;

        ReadOnlyListWrapper<MapLayer> IGridMapContext<BoxPusherContext, ItemReference>.GridLayers() => itemMap.GridLayers();
        ReadOnlyListWrapper<MapLayer> IGridMapContext<BoxPusherContext, ActorReference>.GridLayers() => actorMap.GridLayers();

        public bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<BoxPusherContext, ItemReference> data)
        {
            return itemMap.TryGetGridDataFor(layer, out data);
        }

        public bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<BoxPusherContext, ActorReference> data)
        {
            return actorMap.TryGetGridDataFor(layer, out data);
        }

        public ItemRegistry<BoxPusherContext, ActorReference> ActorRegistry
        {
            get { return actorContext.ItemRegistry; }
        }

        public EntityRegistry<ActorReference> ActorEntities
        {
            get { return actorContext.EntityRegistry; }
        }

        public IItemResolver<BoxPusherContext, ActorReference> ActorResolver
        {
            get { return actorContext.ItemResolver; }
        }
        
        public ItemRegistry<BoxPusherContext, ItemReference> ItemRegistry
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
    }
}