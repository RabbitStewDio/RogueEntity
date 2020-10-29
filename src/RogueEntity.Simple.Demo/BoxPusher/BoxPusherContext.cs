using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    public class BoxPusherContext: IItemContext<BoxPusherContext, ItemReference>, 
                                   IItemContext<BoxPusherContext, ActorReference>,
                                   IGridMapContext<BoxPusherContext, ItemReference>,
                                   IGridMapContext<BoxPusherContext, ActorReference>
    {
        ItemContextBackend<BoxPusherContext, ItemReference> itemContext;
        ItemContextBackend<BoxPusherContext, ActorReference> actorContext;

        DefaultGridPositionContextBackend<BoxPusherContext, ItemReference> itemMap;
        DefaultGridPositionContextBackend<BoxPusherContext, ActorReference> actorMap;

        public BoxPusherContext(int tileWidth, int tileHeight)
        {
            itemContext = new ItemContextBackend<BoxPusherContext, ItemReference>(new ItemReferenceMetaData());
            actorContext = new ItemContextBackend<BoxPusherContext, ActorReference>(new ActorReferenceMetaData());
            
            itemMap = new DefaultGridPositionContextBackend<BoxPusherContext, ItemReference>()
                .WithRawMapLayer(BoxPusherMapLayers.Floor, new DefaultGridMapDataContext<BoxPusherContext, ItemReference>(BoxPusherMapLayers.Floor, tileWidth, tileHeight))
                .WithRawMapLayer(BoxPusherMapLayers.Items, new DefaultGridMapDataContext<BoxPusherContext, ItemReference>(BoxPusherMapLayers.Items, tileWidth, tileHeight));
            actorMap = new DefaultGridPositionContextBackend<BoxPusherContext, ActorReference>()
                .WithRawMapLayer(BoxPusherMapLayers.Actors, new DefaultGridMapDataContext<BoxPusherContext, ActorReference>(BoxPusherMapLayers.Actors, tileWidth, tileHeight));
        }

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

        public bool TryGetGridRawDataFor(MapLayer layer, out IGridMapRawDataContext<ItemReference> data)
        {
            return itemMap.TryGetGridRawDataFor(layer, out data);
        }

        public bool TryGetGridRawDataFor(MapLayer layer, out IGridMapRawDataContext<ActorReference> data)
        {
            return actorMap.TryGetGridRawDataFor(layer, out data);
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