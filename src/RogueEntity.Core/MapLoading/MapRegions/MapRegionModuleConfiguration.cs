using System;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    public class MapRegionModuleConfiguration
    {
        public MapRegionModuleConfiguration()
        {
            MapLoadingTimeout = TimeSpan.FromMilliseconds(500);
            MapEvictionTimer = TimeSpan.FromMilliseconds(500);
        }

        public TimeSpan MapLoadingTimeout { get; set; } 
        public TimeSpan MapEvictionTimer { get; set; } 
    }
}
