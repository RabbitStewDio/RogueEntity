using EnTTSharp.Entities;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Services;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;
using System.Collections.Generic;

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
    }
}
