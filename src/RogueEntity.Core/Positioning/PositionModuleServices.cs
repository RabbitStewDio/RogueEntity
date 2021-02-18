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

namespace RogueEntity.Core.Positioning
{
    public static class PositionModuleServices
    {
        public static DynamicDataViewConfiguration LookupDefaultConfiguration<TEntityId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out IGridMapConfiguration<TEntityId> mapConfig))
            {
                return new DynamicDataViewConfiguration(mapConfig.OffsetX, mapConfig.OffsetY, mapConfig.TileSizeX, mapConfig.TileSizeY);
            }

            if (serviceResolver.TryResolve(out DynamicDataViewConfiguration viewConfig))
            {
                return viewConfig;
            }

            return new DynamicDataViewConfiguration(0, 0, 32, 32);
        }

        public static IGridMapContext<TEntityId> GetOrCreateGridMapContext<TEntityId>(this IServiceResolver serviceResolver)
        {
            return GetOrCreateGridMapContext<TEntityId>(serviceResolver, LookupDefaultConfiguration<TEntityId>(serviceResolver));
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
            return GetOrCreateDefaultGridMapContext<TEntityId>(serviceResolver, LookupDefaultConfiguration<TEntityId>(serviceResolver));
        }

        public static DefaultGridPositionContextBackend<TEntityId> GetOrCreateDefaultGridMapContext<TEntityId>(this IServiceResolver serviceResolver, DynamicDataViewConfiguration config)
        {
            if (serviceResolver.TryResolve(out IGridMapContext<TEntityId> maybeMap))
            {
                if (maybeMap is DefaultGridPositionContextBackend<TEntityId> defaultImpl)
                {
                    return defaultImpl;
                }

                throw new InvalidOperationException("A conflicting grid map implementation has been defined.");
            }

            var created = new DefaultGridPositionContextBackend<TEntityId>(config);
            if (!serviceResolver.TryResolve(out IGridMapConfiguration<TEntityId> _))
            {
                serviceResolver.Store<IGridMapConfiguration<TEntityId>>(new GridMapConfiguration<TEntityId>(config));
            }

            serviceResolver.Store<IGridMapContext<TEntityId>>(created);
            return created;
        }

        public static GridItemPlacementService<TEntityId> GetOrCreateGridItemPlacementService<TEntityId>(this IServiceResolver serviceResolver)
            where TEntityId : IEntityKey
        {
            if (serviceResolver.TryResolve(out GridItemPlacementService<TEntityId> gs))
            {
                return gs;
            }

            var r = new GridItemPlacementService<TEntityId>(serviceResolver.Resolve<IItemResolver<TEntityId>>(),
                                                            serviceResolver.Resolve<IGridMapContext<TEntityId>>());
            serviceResolver.Store(r);
            return r;
        }

        public static GridItemPlacementLocationService<TEntityId> GetOrCreateGridItemPlacementLocationService<TEntityId>(this IServiceResolver serviceResolver)
            where TEntityId : IEntityKey
        {
            if (serviceResolver.TryResolve(out GridItemPlacementLocationService<TEntityId> gs))
            {
                return gs;
            }

            var r = new GridItemPlacementLocationService<TEntityId>(serviceResolver.Resolve<IItemResolver<TEntityId>>(),
                                                                    serviceResolver.Resolve<IGridMapContext<TEntityId>>());
            serviceResolver.Store(r);
            return r;
        }
        
        public static HashSet<MapLayer> CollectMapLayers<TItemId, TAdditionalComponent>(ModuleEntityInitializationParameter<TItemId> initParameter) where TItemId : IEntityKey
        {
            var moduleContext = initParameter.ContentDeclarations;
            var layers = new HashSet<MapLayer>();
            foreach (var bi in moduleContext.DeclaredBulkItems)
            {
                if (bi.itemDeclaration.TryQuery(out IItemComponentDesignTimeInformationTrait<MapLayerPreference> layerPref) &&
                    bi.itemDeclaration.HasItemComponent<TItemId, TAdditionalComponent>() &&
                    layerPref.TryQuery(out var layerPreferences))
                {
                    layers.UnionWith(layerPreferences.AcceptableLayers);
                }
            }

            foreach (var bi in moduleContext.DeclaredReferenceItems)
            {
                if (bi.itemDeclaration.TryQuery(out IItemComponentDesignTimeInformationTrait<MapLayerPreference> layerPref) &&
                    bi.itemDeclaration.HasItemComponent<TItemId, TAdditionalComponent>() &&
                    layerPref.TryQuery(out var layerPreferences))
                {
                    layers.UnionWith(layerPreferences.AcceptableLayers);
                }
            }

            return layers;
        }

        public static HashSet<MapLayer> CollectMapLayers<TItemId>(ModuleEntityInitializationParameter<TItemId> initParameter) where TItemId : IEntityKey
        {
            var moduleContext = initParameter.ContentDeclarations;
            var layers = new HashSet<MapLayer>();
            foreach (var bi in moduleContext.DeclaredBulkItems)
            {
                if (bi.itemDeclaration.TryQuery(out IItemComponentDesignTimeInformationTrait<MapLayerPreference> layerPref) &&
                    layerPref.TryQuery(out var layerPreferences))
                {
                    layers.UnionWith(layerPreferences.AcceptableLayers);
                }
            }

            foreach (var bi in moduleContext.DeclaredReferenceItems)
            {
                if (bi.itemDeclaration.TryQuery(out IItemComponentDesignTimeInformationTrait<MapLayerPreference> layerPref) &&
                    layerPref.TryQuery(out var layerPreferences))
                {
                    layers.UnionWith(layerPreferences.AcceptableLayers);
                }
            }

            return layers;
        }


    }
}
