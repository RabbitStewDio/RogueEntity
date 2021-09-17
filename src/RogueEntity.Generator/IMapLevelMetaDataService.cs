using RogueEntity.Core.Utils;
using RogueEntity.Generator.MapFragments;

namespace RogueEntity.Generator
{
    public interface IMapLevelMetaDataService
    {
        bool TryGetMetaData(int key, out MapFragmentInfo data);
        bool TryGetMapBounds(int key, out Rectangle data);
    }
}
