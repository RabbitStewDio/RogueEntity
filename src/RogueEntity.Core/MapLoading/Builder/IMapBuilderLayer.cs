using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.MapLoading.Builder
{
    public interface IMapBuilderLayer
    {
        public IItemRegistry ItemRegistry { get; }
        public bool Instantiate(ItemDeclarationId item, Position pos, IMapBuilderInstantiationLifter? postProc = null);
        public bool Clear(Position pos, IMapBuilderInstantiationLifter? postProc = null);
    }
}
