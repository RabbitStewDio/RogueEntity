using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.MapLoading.PlayerSpawning
{
    public class FlatLevelMapAvailabilityService : IMapAvailabilityService
    {
        readonly IMapRegionLoaderService<int> loaderService;

        public FlatLevelMapAvailabilityService(IMapRegionLoaderService<int> loaderService)
        {
            this.loaderService = loaderService;
        }

        public bool IsLevelReadyForSpawning(int zPosition)
        {
            return loaderService.IsRegionLoaded(zPosition);
        }

        public bool IsLevelPositionAvailable<TPosition>(TPosition p)
            where TPosition : IPosition<TPosition>
        {
            return loaderService.IsRegionLoaded(p.GridZ);
        }
        
    }
}
