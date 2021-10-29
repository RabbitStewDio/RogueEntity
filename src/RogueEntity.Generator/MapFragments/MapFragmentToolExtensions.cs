using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Generator.MapFragments;

namespace RogueEntity.Generator
{
    public static class MapFragmentToolExtensions
    {
        public static MapFragmentTool ForFragmentPlacement(this MapBuilder b, IEntityRandomGeneratorSource randomContext)
        {
            return new MapFragmentTool(b, randomContext);
        }
        
    }
}
