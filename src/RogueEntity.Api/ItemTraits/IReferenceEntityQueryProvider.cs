using EnTTSharp.Entities;
using System.Collections.Generic;

namespace RogueEntity.Api.ItemTraits
{
    public interface IReferenceEntityQueryProvider<TEntityKey>
        where TEntityKey: IEntityKey
    {
        IEnumerable<TEntityKey> QueryById(ItemDeclarationId id);
        IEnumerable<(TEntityKey, TEntityTraitA)> QueryByTrait<TEntityTraitA>();
        IEnumerable<(TEntityKey, TEntityTraitA, TEntityTraitB)> QueryByTrait<TEntityTraitA, TEntityTraitB>();
    }
}
