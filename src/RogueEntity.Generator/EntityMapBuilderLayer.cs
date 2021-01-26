using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Generator
{
    public abstract class EntityMapBuilderLayer
    {
        public abstract IItemRegistry ItemRegistry { get; }
        public abstract bool Instantiate(ItemDeclarationId item, Position pos, IMapBuilderInstantiationLifter postProc = null);
        public abstract bool Clear(Position pos, IMapBuilderInstantiationLifter postProc = null);
    }
}
