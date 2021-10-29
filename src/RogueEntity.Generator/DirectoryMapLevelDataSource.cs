using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.Storage;
using RogueEntity.Generator.MapFragments;
using System;
using System.Collections.Generic;

namespace RogueEntity.Generator
{
    /// <summary>
    ///   A map/level data source that uses a directory in the local file system as data source. 
    /// </summary>
    public class DirectoryMapLevelDataSource : StaticMapLevelDataSource
    {
        readonly IStorageLocationService storageLocations;

        public DirectoryMapLevelDataSource([NotNull] Lazy<MapBuilder> mapBuilder,
                                           [NotNull] IStorageLocationService storageLocations,
                                           [NotNull] IEntityRandomGeneratorSource randomSource) : base(mapBuilder, randomSource)
        {
            this.storageLocations = storageLocations ?? throw new ArgumentNullException(nameof(storageLocations));
        }

        protected override IEnumerable<MapFragment> LoadAvailableMapFragments()
        {
            var registry = new MapFragmentRegistry("*.map.yml");
            registry.LoadAll(new MapFragmentParser(), storageLocations.ContentLocation);
            return registry.Items;
        }
    }
}
