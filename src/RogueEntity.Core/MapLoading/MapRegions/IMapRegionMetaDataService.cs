using RogueEntity.Core.Utils;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    public interface IMapRegionMetaDataService<TRegionKey>
    {
        bool TryGetRegionBounds(TRegionKey key, out Rectangle3D data);
    }
}
