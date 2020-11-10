using System.Collections.Generic;

namespace RogueEntity.Core.Infrastructure.ItemTraits
{
    /// <summary>
    ///   The behaviour for an item in the world. Implementations of this class
    ///   are shared across all items of a given type. 
    /// </summary>
    public interface IItemDeclaration : IWorldEntity
    {
        ItemDeclarationId Id { get; }
        bool TryQuery<TTrait>(out TTrait t) where TTrait : IItemTrait;
        List<TTrait> QueryAll<TTrait>(List<TTrait> cache = null) where TTrait : IItemTrait;
        
        public IEnumerable<EntityRoleInstance> GetEntityRoles();
        public IEnumerable<EntityRelationInstance> GetEntityRelations();
    }
}