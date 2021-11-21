using System.Collections.Generic;

namespace RogueEntity.Api.ItemTraits
{
    public interface IReferenceEntityQueryProvider<TEntityKey>
    {
        IEnumerable<TEntityKey> QueryById(ItemDeclarationId id);
        IEnumerable<(TEntityKey, TEntityTraitA)> QueryByTrait<TEntityTraitA>();
        IEnumerable<(TEntityKey, TEntityTraitA, TEntityTraitB)> QueryByTrait<TEntityTraitA, TEntityTraitB>();
    }
}
