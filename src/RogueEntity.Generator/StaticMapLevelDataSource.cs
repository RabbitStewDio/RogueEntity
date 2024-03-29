using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Generator.MapFragments;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Generator
{
    public abstract class StaticMapLevelDataSource : IMapRegionLoadingStrategy<int>,
                                                     IMapRegionMetaDataService<int>
    {
        static readonly ILogger Logger = SLog.ForContext<StaticMapLevelDataSource>();

        readonly IEntityRandomGeneratorSource randomSource;
        readonly Lazy<MapBuilder> mapBuilder;
        readonly List<MapFragment> levelData;
        bool initialized;

        protected StaticMapLevelDataSource(Lazy<MapBuilder> mapBuilder,
                                           IEntityRandomGeneratorSource randomSource)
        {
            this.randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
            this.mapBuilder = mapBuilder ?? throw new ArgumentNullException(nameof(mapBuilder));
            this.levelData = new List<MapFragment>();
            this.initialized = false;
        }

        public virtual void Initialize()
        {
            this.levelData.Clear();
            this.levelData.AddRange(LoadAvailableMapFragments().OrderBy(e => e.Info.Name));
            this.initialized = true;
            Logger.Information("Found {ChunkCount} chunks", levelData.Count);
        }

        public bool Initialized => initialized;

        public int Count => levelData.Count;

        public MapRegionProcessingResult PerformLoadChunk(int region)
        {
            if (!levelData.GetItemAt(region).TryGetValue(out var mapFragment))
            {
                if (!initialized)
                {
                    Logger.Error("Level data source has not been initialized yet - Check your initialization routines");
                }
                else
                {
                    Logger.Debug("Unable to serve request for region {RegionKey} - no such level data", region);
                }

                return MapRegionProcessingResult.Error;
            }

            mapBuilder.Value.ForFragmentPlacement(randomSource).CopyToMap(mapFragment, EntityGridPosition.Of(MapLayer.Indeterminate, 0, 0, region));
            return MapRegionProcessingResult.Success;
        }

        public bool TryGetRegionBounds(int key, out Rectangle3D data)
        {
            if (levelData.GetItemAt(key).TryGetValue(out var mapFragment))
            {
                data = new Rectangle3D(0, 0, key, mapFragment.Size.Width, mapFragment.Size.Height, 1);
                return true;
            }

            data = default;
            return false;
        }

        public bool TryGetMetaData(int key, out MapFragmentInfo data)
        {
            if (levelData.GetItemAt(key).TryGetValue(out var mapFragment))
            {
                data = mapFragment.Info;
                return true;
            }

            data = default;
            return false;
        }

        protected abstract IEnumerable<MapFragment> LoadAvailableMapFragments();
    }
}
