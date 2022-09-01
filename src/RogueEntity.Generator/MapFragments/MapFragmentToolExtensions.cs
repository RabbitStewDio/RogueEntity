using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.Builder;

namespace RogueEntity.Generator.MapFragments
{
    public static class MapFragmentToolExtensions
    {
        public static MapFragmentTool ForFragmentPlacement(this MapBuilder b, IEntityRandomGeneratorSource randomContext)
        {
            return new MapFragmentTool(b, randomContext);
        }
        
    }
}
