using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Storage;
using RogueEntity.Core.Utils;
using RogueEntity.Generator;
using RogueEntity.Generator.MapFragments;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Samples.BoxPusher.Core
{
    public class BoxPusherMapLevelDataSource : MapRegionLoaderServiceBase<int>
    {
        readonly MapFragmentRegistry registry;
        readonly MapFragmentTool mapBuilder;
        readonly List<MapFragment> levelData;

        public BoxPusherMapLevelDataSource(MapBuilder mapBuilder,
                                           MapFragmentParser mapFragmentParser,
                                           IStorageLocationService storageLocations,
                                           IEntityRandomGeneratorSource randomSource)
        {
            this.mapBuilder = mapBuilder.ForFragmentPlacement(randomSource);

            this.registry = new MapFragmentRegistry();
            this.registry.LoadAll(mapFragmentParser, storageLocations.ContentLocation);

            this.levelData = this.registry.Items
                                 .OrderBy(e => e.Info.Name)
                                 .ToList();
        }

        public int Count => levelData.Count;
        
        protected override MapRegionLoadingStatus PerformLoadNextChunk(int region)
        {
            if (!levelData.GetItemAt(region).TryGetValue(out var mapFragment))
            {
                return MapRegionLoadingStatus.Error;
            }

            mapBuilder.CopyToMap(mapFragment, EntityGridPosition.Of(MapLayer.Indeterminate, 0, 0, region));
            return MapRegionLoadingStatus.Loaded;
        }


        public override bool IsLevelPositionAvailable<TPosition>(TPosition p)
        {
            return IsLoaded(p.GridZ);
        }
    }
}
