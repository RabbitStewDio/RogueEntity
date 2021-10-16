using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Positioning;
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
    public abstract class StaticMapLevelDataSource : MapRegionLoaderServiceBase<int>, IMapLevelMetaDataService, IMapAvailabilityService
    {
        static readonly ILogger Logger = SLog.ForContext<StaticMapLevelDataSource>();

        readonly IEntityRandomGeneratorSource randomSource;
        readonly Lazy<MapBuilder> mapBuilder;
        readonly List<MapFragment> levelData;
        bool initialized;

        protected StaticMapLevelDataSource(Lazy<MapBuilder> mapBuilder, IEntityRandomGeneratorSource randomSource)
        {
            this.randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
            this.mapBuilder = mapBuilder ?? throw new ArgumentNullException(nameof(mapBuilder));
            this.levelData = new List<MapFragment>();
            this.initialized = false;
        }

        public override void Initialize()
        {
            this.levelData.Clear();
            this.levelData.AddRange(LoadMapFragments().OrderBy(e => e.Info.Name));
            this.initialized = true;
            this.mapBuilder.Value.MapLayerDirty += OnMapLayerDirty;
            Logger.Information("Found {ChunkCount} chunks", levelData.Count);
        }

        void OnMapLayerDirty(object sender, MapRegionDirtyEventArgs e)
        {
            for (int z = e.ZPositionFrom; z <= e.ZPositionTo; z += 1)
            {
                this.EvictRegion(z);
            }
        }

        public int Count => levelData.Count;

        protected override MapRegionLoadingStatus PerformLoadNextChunk(int region)
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
                return MapRegionLoadingStatus.Error;
            }

            mapBuilder.Value.ForFragmentPlacement(randomSource)
                      .CopyToMap(mapFragment, EntityGridPosition.Of(MapLayer.Indeterminate, 0, 0, region));
            return MapRegionLoadingStatus.Loaded;
        }


        public bool TryGetMapBounds(int key, out Rectangle data)
        {
            if (levelData.GetItemAt(key).TryGetValue(out var mapFragment))
            {
                data = new Rectangle(0, 0, mapFragment.Size.Width, mapFragment.Size.Height);
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

        public bool IsLevelReadyForSpawning(int zPosition)
        {
            return IsRegionLoaded(zPosition);
        }

        public bool IsLevelPositionAvailable<TPosition>(TPosition p)
            where TPosition : IPosition<TPosition>
        {
            return IsRegionLoaded(p.GridZ);
        }

        protected abstract IEnumerable<MapFragment> LoadMapFragments();
    }
}
