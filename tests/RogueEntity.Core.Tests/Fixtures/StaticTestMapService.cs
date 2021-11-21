using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.MapLoading.FlatLevelMaps;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.MapLoading.PlayerSpawning;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Tests.Players;
using RogueEntity.Core.Utils;
using RogueEntity.Generator;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Tests.Fixtures
{
    public class StaticTestMapService : IMapRegionLoadingStrategy<int>,
                                        IMapRegionEvictionStrategy<int>,
                                        IFlatLevelPlayerSpawnInformationSource,
                                        IMapRegionMetaDataService<int>
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

        public bool TryCreateInitialLevelRequest(in PlayerTag player, out int level)
        {
            level = defaultLevel;
            return true;
        }

        public void AddMap(int z, string mapDataRaw)
        {
            mapData[z] = mapDataRaw;
        }

        public void AddMapToken(string tokenCharacter, params ItemDeclarationId[] tokenData)
        {
            tokens[tokenCharacter] = tokenData;
        }

        public bool TryGetRegionBounds(int region, out Rectangle3D data)
        {
            if (!mapData.TryGetValue(region, out var raw))
            {
                data = default;
                return false;
            }

            var tokenParser = new TokenParser();
            foreach (var t in tokens)
            {
                tokenParser.AddToken(t.Key, t.Value);
            }

            var map = TestHelpers.Parse<ItemDeclarationId[]>(raw, tokenParser, out var mapBounds);
            data = new Rectangle3D(mapBounds.X, mapBounds.Y, region, mapBounds.Width, mapBounds.Height, 1);
            return true;
        }

        public MapRegionProcessingResult PerformLoadChunk(int region)
        {
            if (!mapData.TryGetValue(region, out var raw))
            {
                return MapRegionProcessingResult.Error;
            }

            var tokenParser = new TokenParser();
            foreach (var t in tokens)
            {
                tokenParser.AddToken(t.Key, t.Value);
            }

            var map = TestHelpers.Parse<ItemDeclarationId[]>(raw, tokenParser, out var mapBounds);
            var mf = new TestMapFragmentTool(mapBuilder.Value, new FixedRandomGeneratorSource(100));
            mf.CopyToMap(map, mapBounds, EntityGridPosition.Of(MapLayer.Indeterminate, 0, 0, region));
            return MapRegionProcessingResult.Success;
        }

        public MapRegionProcessingResult PerformUnloadChunk(int key)
        {
            return MapRegionProcessingResult.Invalid;
        }
    }
}
