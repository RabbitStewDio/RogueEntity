using RogueEntity.Core.Utils;

namespace RogueEntity.Generator
{
    public interface IMapRegionMetaDataService<TRegionKey>
    {
        bool TryGetRegionBounds(TRegionKey key, out Rectangle3D data);
    }
}
