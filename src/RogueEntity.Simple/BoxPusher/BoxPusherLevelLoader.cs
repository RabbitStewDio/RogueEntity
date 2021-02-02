using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Chunks;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Generator;
using RogueEntity.Generator.MapFragments;
using System;

namespace RogueEntity.Simple.BoxPusher
{
    public class BoxPusherMapLevelDataSource : MapLevelDataSourceBase<int, Unit>
    {
        readonly MapBuilder mapBuilder;
        readonly MapFragmentParser mapFragmentParser;
        readonly IEntityRandomGeneratorSource randomSource;

        public BoxPusherMapLevelDataSource(MapBuilder mapBuilder, MapFragmentParser mapFragmentParser, IEntityRandomGeneratorSource randomSource)
        {
            this.mapBuilder = mapBuilder;
            this.mapFragmentParser = mapFragmentParser;
            this.randomSource = randomSource;
        }

        public BoxPusherMapLevelDataSource(TimeSpan maximumProcessingTime, 
                                           MapBuilder mapBuilder, 
                                           MapFragmentParser mapFragmentParser, 
                                           IEntityRandomGeneratorSource randomSource) : base(maximumProcessingTime)
        {
            this.mapBuilder = mapBuilder;
            this.mapFragmentParser = mapFragmentParser;
            this.randomSource = randomSource;
        }

        public override bool CanCreateLevel(int key)
        {
            return mapFragmentParser.TryParseFromFile($"BoxPusher/Content/Maps.Level_{key:D5}.boxlevel", out _);
        }

        protected override Optional<Unit> PerformLoadChunks(in int key, in Optional<Unit> progressSoFar)
        {
            if (mapFragmentParser.TryParseFromFile($"BoxPusher/Content/Maps.Level_{key:D5}.boxlevel", out var file))
            {
                mapBuilder.ForFragmentPlacement(randomSource).CopyToMap(file, EntityGridPosition.Of(MapLayer.Indeterminate, 0, 0, key));
            }

            return Optional.Empty();
        }
    }
}
