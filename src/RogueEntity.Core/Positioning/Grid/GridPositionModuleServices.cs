using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Services;
using RogueEntity.Core.Utils.DataViews;
using System;

namespace RogueEntity.Core.Positioning.Grid
{
    public static class GridPositionModuleServices
    {
                public static IGridMapContext<TEntityId> GetOrCreateGridMapContext<TEntityId>(this IServiceResolver serviceResolver)
        {
            return GetOrCreateGridMapContext<TEntityId>(serviceResolver, PositionModuleServices.LookupDefaultConfiguration<TEntityId>(serviceResolver));
        }

        public static IGridMapContext<TEntityId> GetOrCreateGridMapContext<TEntityId>(this IServiceResolver serviceResolver, DynamicDataViewConfiguration config)
        {
            if (serviceResolver.TryResolve(out IGridMapContext<TEntityId> map))
            {
                return map;
            }

            map = new DefaultGridPositionContextBackend<TEntityId>(config);
            if (!serviceResolver.TryResolve(out IGridMapConfiguration<TEntityId> _))
            {
                serviceResolver.Store<IGridMapConfiguration<TEntityId>>(new GridMapConfiguration<TEntityId>(config));
            }

            serviceResolver.Store(map);
            return map;
        }

        public static DefaultGridPositionContextBackend<TEntityId> GetOrCreateDefaultGridMapContext<TEntityId>(this IServiceResolver serviceResolver)
        {
            return GetOrCreateDefaultGridMapContext<TEntityId>(serviceResolver, PositionModuleServices.LookupDefaultConfiguration<TEntityId>(serviceResolver));
        }

        public static bool TryGetOrCreateDefaultMapServices<TEntityId>(IServiceResolver serviceResolver,
                                                                       out DefaultGridPositionContextBackend<TEntityId> map,
                                                                       out ItemPlacementServiceContext<TEntityId> placementService)
        {
            if (!TryGetOrCreateDefaultGridMapContext(serviceResolver,
                                                     PositionModuleServices.LookupDefaultConfiguration<TEntityId>(serviceResolver),
                                                     out map))
            {
                placementService = default;
                return false;
            }

            if (serviceResolver.TryResolve(out IItemPlacementServiceContext<TEntityId> ctx))
            {
                if (ctx is ItemPlacementServiceContext<TEntityId> ps)
                {
                    placementService = ps;
                    return true;
                }

                placementService = default;
                return false;
            }

            placementService = new ItemPlacementServiceContext<TEntityId>();
            serviceResolver.Store<IItemPlacementServiceContext<TEntityId>>(placementService);
            return true;
        }

        static bool TryGetOrCreateDefaultGridMapContext<TEntityId>(this IServiceResolver serviceResolver, DynamicDataViewConfiguration config, out DefaultGridPositionContextBackend<TEntityId> map)
        {
            if (serviceResolver.TryResolve(out IGridMapContext<TEntityId> maybeMap))
            {
                if (maybeMap is DefaultGridPositionContextBackend<TEntityId> defaultImpl)
                {
                    map = defaultImpl;
                    return true;
                }

                map = default;
                return false;
            }

            var created = new DefaultGridPositionContextBackend<TEntityId>(config);
            if (!serviceResolver.TryResolve(out IGridMapConfiguration<TEntityId> _))
            {
                serviceResolver.Store<IGridMapConfiguration<TEntityId>>(new GridMapConfiguration<TEntityId>(config));
            }

            serviceResolver.Store<IGridMapContext<TEntityId>>(created);
            map = created;
            return true;
        }

        public static DefaultGridPositionContextBackend<TEntityId> GetOrCreateDefaultGridMapContext<TEntityId>(this IServiceResolver serviceResolver, DynamicDataViewConfiguration config)
        {
            if (!TryGetOrCreateDefaultGridMapContext<TEntityId>(serviceResolver, config, out var result))
            {
                throw new InvalidOperationException("A conflicting grid map implementation has been defined.");
            }

            return result;
        }

        public static IItemPlacementService<TEntityId> GetOrCreateGridItemPlacementService<TEntityId>(this IServiceResolver serviceResolver)
            where TEntityId : IEntityKey
        {
            if (serviceResolver.TryResolve(out IItemPlacementService<TEntityId> gs))
            {
                return gs;
            }

            var r = new GridItemPlacementService<TEntityId>(serviceResolver.Resolve<IItemResolver<TEntityId>>(),
                                                            serviceResolver.Resolve<IGridMapContext<TEntityId>>());
            serviceResolver.Store(r);
            serviceResolver.Store<IItemPlacementService<TEntityId>>(r);
            return r;
        }

        public static IItemPlacementLocationService<TEntityId> GetOrCreateGridItemPlacementLocationService<TEntityId>(this IServiceResolver serviceResolver)
            where TEntityId : IEntityKey
        {
            if (serviceResolver.TryResolve(out IItemPlacementLocationService<TEntityId> gs))
            {
                return gs;
            }

            var r = new GridItemPlacementLocationService<TEntityId>(serviceResolver.Resolve<IItemResolver<TEntityId>>(),
                                                                    serviceResolver.Resolve<IGridMapContext<TEntityId>>());
            serviceResolver.Store(r);
            serviceResolver.Store<IItemPlacementLocationService<TEntityId>>(r);
            return r;
        }

    }
}
