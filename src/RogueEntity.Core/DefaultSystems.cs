using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Services;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core
{
    public static class DefaultSystems
    {
        public static void ConfigureEntity<TEntityId>(this IServiceResolver serviceResolver, IBulkDataStorageMetaData<TEntityId> meta, params MapLayer[] layers)
            where TEntityId : IEntityKey
        {
            serviceResolver.ConfigureEntityType(meta);
            if (layers.Length == 0)
            {
                return;
            }

            ConfigureEntityMap<TEntityId>(serviceResolver, layers);
        }

        static void ConfigureEntityMap<TEntityId>(IServiceResolver serviceResolver, MapLayer[] layers) where TEntityId : IEntityKey
        {
            var mapContext = serviceResolver.GetOrCreateDefaultGridMapContext<TEntityId>();
            var actorPlacementContext = new ItemPlacementServiceContext<TEntityId>();
            var itemPlacementService = serviceResolver.GetOrCreateGridItemPlacementService<TEntityId>();
            var locationService = serviceResolver.GetOrCreateGridItemPlacementLocationService<TEntityId>();

            foreach (var layer in layers)
            {
                mapContext.WithDefaultMapLayer(layer);
                actorPlacementContext.WithLayer(layer,
                                                itemPlacementService,
                                                locationService);
            }

            serviceResolver.Store<IItemPlacementServiceContext<TEntityId>>(actorPlacementContext);
        }
    }
}
