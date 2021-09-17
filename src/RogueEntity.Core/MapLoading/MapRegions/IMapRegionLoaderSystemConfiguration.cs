using System;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    public interface IMapRegionLoaderSystemConfiguration
    {
        public TimeSpan MapLoadingTimeout { get; } 
    }
}
