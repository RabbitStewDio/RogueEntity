using EnTTSharp;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using Serilog;
using System;

namespace RogueEntity.Core.MapLoading.FlatLevelMaps
{
    public class FlatLevelRegionEvictionStrategy : IMapRegionEvictionStrategy<int>
    {
        readonly ILogger logger = SLog.ForContext<FlatLevelRegionEvictionStrategy>();
        readonly SelectForPreservationHandler selectForPreservationHandler;
        readonly Lazy<MapBuilder> mapBuilder;
        readonly IMapRegionMetaDataService<int> mapMetaData;

        public FlatLevelRegionEvictionStrategy(Lazy<MapBuilder> mapBuilder, IMapRegionMetaDataService<int> mapMetaData)
        {
            this.mapBuilder = mapBuilder;
            this.mapMetaData = mapMetaData;
            this.selectForPreservationHandler = new SelectForPreservationHandler();
        }

        public MapRegionProcessingResult PerformUnloadChunk(int region)
        {
            if (!mapMetaData.TryGetRegionBounds(region, out var bounds))
            {
                return MapRegionProcessingResult.Error;
            }

            logger.Debug("Unloading map region {Region}", region);
            
            var mb = mapBuilder.Value;
            mb.Clear(MapLayer.Indeterminate, region, bounds.ToLayerSlice(), selectForPreservationHandler);
            return MapRegionProcessingResult.Success;
        }

        class SelectForPreservationHandler : IMapBuilderInstantiationLifter
        {
            public Optional<TEntity> ClearPreProcess<TEntity>(ItemDeclarationId item, Position pos, IItemResolver<TEntity> itemResolver, TEntity entityKey)
                where TEntity : struct, IEntityKey
            {
                if (!itemResolver.TryQueryData(entityKey, out EvictionBehaviour eb))
                {
                    eb = EvictionBehaviour.Destroy;
                }

                if (eb == EvictionBehaviour.Destroy)
                {
                    return entityKey;
                }

                return Optional.Empty();
            }

            public Optional<TEntity> InstantiatePostProcess<TEntity>(ItemDeclarationId item, Position pos, IItemResolver<TEntity> itemResolver, TEntity entityKey)
                where TEntity : struct, IEntityKey
            {
                throw new NotImplementedException();
            }
        }

    }
}
