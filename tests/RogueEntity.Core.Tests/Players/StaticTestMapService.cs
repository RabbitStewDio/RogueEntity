using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Tests.Players
{
    public class StaticTestMapService : MapRegionLoaderServiceBase<int>, IMapAvailabilityService, IPlayerSpawnInformationSource
    {
        readonly Lazy<MapBuilder> mapBuilder;
        readonly int defaultLevel;
        readonly Dictionary<int, string> mapData;
        readonly Dictionary<string, ItemDeclarationId[]> tokens;

        public StaticTestMapService(Lazy<MapBuilder> mapBuilder,
                                int defaultLevel)
        {
            this.mapBuilder = mapBuilder;
            this.defaultLevel = defaultLevel;
            this.mapData = new Dictionary<int, string>();
            this.tokens = new Dictionary<string, ItemDeclarationId[]>();
        }

        public override void Initialize()
        {
            this.mapBuilder.Value.MapLayerDirty += OnMapLayerDirty;
        }

        void OnMapLayerDirty(object sender, MapRegionDirtyEventArgs e)
        {
            for (int z = e.ZPositionFrom; z <= e.ZPositionTo; z += 1)
            {
                this.EvictRegion(z);
            }
        }

        public bool TryCreateInitialLevelRequest(in PlayerTag player, out int level)
        {
            level = defaultLevel;
            return true;
        }

        public bool IsLevelPositionAvailable<TPosition>(TPosition p)
            where TPosition : IPosition<TPosition>
        {
            return IsRegionLoaded(p.GridZ);
        }

        public bool IsLevelReadyForSpawning(int zPosition)
        {
            return IsRegionLoaded(zPosition);
        }

        public void AddMap(int z, string mapDataRaw)
        {
            mapData[z] = mapDataRaw;
        }

        public void AddMapToken(string tokenCharacter, params ItemDeclarationId[] tokenData)
        {
            tokens[tokenCharacter] = tokenData;
        }
        
        protected override MapRegionLoadingStatus PerformLoadNextChunk(int region)
        {
            if (!mapData.TryGetValue(region, out var raw))
            {
                return MapRegionLoadingStatus.Error;
            }

            var tokenParser = new TokenParser();
            foreach (var t in tokens)
            {
                tokenParser.AddToken(t.Key, t.Value);
            }

            var map = TestHelpers.Parse<ItemDeclarationId[]>(raw, tokenParser, out var mapBounds);
            var mf = new TestMapFragmentTool(mapBuilder.Value, new FixedRandomGeneratorSource(100));
            mf.CopyToMap(map, mapBounds, EntityGridPosition.Of(MapLayer.Indeterminate, 0, 0, region));
            return MapRegionLoadingStatus.Loaded;
        }
    }
}
