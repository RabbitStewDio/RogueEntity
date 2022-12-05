using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Services;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Positioning
{
    public static class PositionModuleServices
    {
        public static DynamicDataViewConfiguration LookupDefaultConfiguration<TEntityId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve<IGridMapConfiguration<TEntityId>>(out var mapConfig))
            {
                return new DynamicDataViewConfiguration(mapConfig.OffsetX, mapConfig.OffsetY, mapConfig.TileSizeX, mapConfig.TileSizeY);
            }

            if (serviceResolver.TryResolve(out DynamicDataViewConfiguration viewConfig))
            {
                return viewConfig;
            }

            return new DynamicDataViewConfiguration(0, 0, 32, 32);
        }

        public static DynamicDataViewConfiguration LookupDefaultConfiguration(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out DynamicDataViewConfiguration viewConfig))
            {
                return viewConfig;
            }

            return new DynamicDataViewConfiguration(0, 0, 32, 32);
        }


        public static HashSet<MapLayer> CollectMapLayers<TItemId, TAdditionalComponent>(ModuleEntityInitializationParameter<TItemId> initParameter)
            where TItemId : struct, IEntityKey
        {
            var moduleContext = initParameter.ContentDeclarations;
            var layers = new HashSet<MapLayer>();
            foreach (var bi in moduleContext.DeclaredBulkItems)
            {
                if (bi.itemDeclaration.TryQuery<IItemComponentDesignTimeInformationTrait<MapLayerPreference>>(out var layerPref) &&
                    bi.itemDeclaration.HasItemComponent<TItemId, TAdditionalComponent>() &&
                    layerPref.TryQuery(out var layerPreferences))
                {
                    layers.UnionWith(layerPreferences.AcceptableLayers);
                }
            }

            foreach (var bi in moduleContext.DeclaredReferenceItems)
            {
                if (bi.itemDeclaration.TryQuery<IItemComponentDesignTimeInformationTrait<MapLayerPreference>>(out var layerPref) &&
                    bi.itemDeclaration.HasItemComponent<TItemId, TAdditionalComponent>() &&
                    layerPref.TryQuery(out var layerPreferences))
                {
                    layers.UnionWith(layerPreferences.AcceptableLayers);
                }
            }

            return layers;
        }

        public static HashSet<MapLayer> CollectMapLayers<TItemId>(ModuleEntityInitializationParameter<TItemId> initParameter)
            where TItemId : struct, IEntityKey
        {
            var moduleContext = initParameter.ContentDeclarations;
            var layers = new HashSet<MapLayer>();
            foreach (var bi in moduleContext.DeclaredBulkItems)
            {
                if (bi.itemDeclaration.TryQuery<IItemComponentDesignTimeInformationTrait<MapLayerPreference>>(out var layerPref) &&
                    layerPref.TryQuery(out var layerPreferences))
                {
                    layers.UnionWith(layerPreferences.AcceptableLayers);
                }
            }

            foreach (var bi in moduleContext.DeclaredReferenceItems)
            {
                if (bi.itemDeclaration.TryQuery<IItemComponentDesignTimeInformationTrait<MapLayerPreference>>(out var layerPref) &&
                    layerPref.TryQuery(out var layerPreferences))
                {
                    layers.UnionWith(layerPreferences.AcceptableLayers);
                }
            }

            return layers;
        }
        
       public static IMapContext<TEntityId> GetOrCreateGridMapContext<TEntityId>(this IServiceResolver serviceResolver)
        {
            return GetOrCreateGridMapContext<TEntityId>(serviceResolver, PositionModuleServices.LookupDefaultConfiguration<TEntityId>(serviceResolver));
        }

        public static IMapContext<TEntityId> GetOrCreateGridMapContext<TEntityId>(this IServiceResolver serviceResolver, DynamicDataViewConfiguration config)
        {
            if (serviceResolver.TryResolve<IMapContext<TEntityId>>(out var map))
            {
                return map;
            }

            map = new DefaultMapContext<TEntityId>(config);
            if (!serviceResolver.TryResolve<IGridMapConfiguration<TEntityId>>(out _))
            {
                serviceResolver.Store<IGridMapConfiguration<TEntityId>>(new GridMapConfiguration<TEntityId>(config));
            }

            serviceResolver.Store(map);
            return map;
        }

        public static DefaultMapContext<TEntityId> GetOrCreateDefaultMapContext<TEntityId>(this IServiceResolver serviceResolver)
        {
            return GetOrCreateDefaultMapContext<TEntityId>(serviceResolver, PositionModuleServices.LookupDefaultConfiguration<TEntityId>(serviceResolver));
        }

        public static bool TryGetOrCreateDefaultMapServices<TEntityId>(IServiceResolver serviceResolver,
                                                                       [MaybeNullWhen(false)] out DefaultMapContext<TEntityId> map,
                                                                       [MaybeNullWhen(false)] out ItemPlacementServiceContext<TEntityId> placementService)
        {
            if (!TryGetOrCreateDefaultMapContext(serviceResolver,
                                                     PositionModuleServices.LookupDefaultConfiguration<TEntityId>(serviceResolver),
                                                     out map))
            {
                placementService = default;
                return false;
            }

            if (serviceResolver.TryResolve<IItemPlacementServiceContext<TEntityId>>(out var ctx))
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

        static bool TryGetOrCreateDefaultMapContext<TEntityId>(this IServiceResolver serviceResolver,
                                                                   DynamicDataViewConfiguration config,
                                                                   [MaybeNullWhen(false)] out DefaultMapContext<TEntityId> map)
        {
            if (serviceResolver.TryResolve<IMapContext<TEntityId>>(out var maybeMap))
            {
                if (maybeMap is DefaultMapContext<TEntityId> defaultImpl)
                {
                    map = defaultImpl;
                    return true;
                }

                map = default;
                return false;
            }

            var created = new DefaultMapContext<TEntityId>(config);
            if (!serviceResolver.TryResolve<IGridMapConfiguration<TEntityId>>(out _))
            {
                serviceResolver.Store<IGridMapConfiguration<TEntityId>>(new GridMapConfiguration<TEntityId>(config));
            }

            serviceResolver.Store<IMapContext<TEntityId>>(created);
            map = created;
            return true;
        }

        public static DefaultMapContext<TEntityId> GetOrCreateDefaultMapContext<TEntityId>(this IServiceResolver serviceResolver, DynamicDataViewConfiguration config)
        {
            if (!TryGetOrCreateDefaultMapContext<TEntityId>(serviceResolver, config, out var result))
            {
                throw new InvalidOperationException("A conflicting grid map implementation has been defined.");
            }

            return result;
        }

        public static IItemPlacementService<TEntityId> GetOrCreateItemPlacementService<TEntityId>(this IServiceResolver serviceResolver)
            where TEntityId : struct, IEntityKey
        {
            if (serviceResolver.TryResolve<IItemPlacementService<TEntityId>>(out var gs))
            {
                return gs;
            }

            var r = new ItemPlacementService<TEntityId>(serviceResolver.Resolve<IItemResolver<TEntityId>>(),
                                                        serviceResolver.Resolve<IMapContext<TEntityId>>());
            serviceResolver.Store(r);
            serviceResolver.Store<IItemPlacementService<TEntityId>>(r);
            return r;
        }

        public static IItemPlacementLocationService<TEntityId> GetOrCreateItemPlacementLocationService<TEntityId>(this IServiceResolver serviceResolver)
            where TEntityId : struct, IEntityKey
        {
            if (serviceResolver.TryResolve<IItemPlacementLocationService<TEntityId>>(out var gs))
            {
                return gs;
            }

            var r = new ItemPlacementLocationService<TEntityId>(serviceResolver.Resolve<IItemResolver<TEntityId>>(),
                                                                       serviceResolver.Resolve<IMapContext<TEntityId>>());
            serviceResolver.Store(r);
            serviceResolver.Store<IItemPlacementLocationService<TEntityId>>(r);
            return r;
        }
    }
}
