using System.Collections.Generic;
using RogueEntity.Api.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RogueEntity.Api.ItemTraits
{
    /// <summary>
    ///   The behaviour for an item in the world. Implementations of this class
    ///   are shared across all items of a given type. 
    /// </summary>
    public interface IItemDeclaration : IWorldEntity
    {
        ItemDeclarationId Id { get; }
        bool TryQuery<TTrait>([MaybeNullWhen(false)] out TTrait t) where TTrait : IItemTrait;
        BufferList<TTrait> QueryAll<TTrait>(BufferList<TTrait>? cache = null) where TTrait : IItemTrait;
        
        public IEnumerable<(ItemTraitId traitId, EntityRoleInstance role)> GetEntityRoles();
        public IEnumerable<(ItemTraitId traitId, EntityRelationInstance relation)> GetEntityRelations();
    }

    public static class ItemDeclarationExtensions
    {
        public static bool HasRole(this IItemDeclaration d, EntityRoleInstance role)
        {
            return d.GetEntityRoles().Any(e => e.role == role);
        }
    } 
}