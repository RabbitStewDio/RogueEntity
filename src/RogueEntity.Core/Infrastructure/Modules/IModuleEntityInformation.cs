using System;
using System.Collections.Generic;
using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public interface IModuleEntityInformation
    {
        IEnumerable<EntityRole> Roles { get; }
        IEnumerable<EntityRelation> Relations { get; }
        
        /// <summary>
        ///   Validates whether any entity anywhere in the system has the given role assigned.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool RoleExists(EntityRole role);
        
        /// <summary>
        ///   Validates whether this entity type has the given role assigned.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool HasRole(EntityRole role);
        
        public bool HasRole(EntityRole role, EntityRole requiredRole);
        public bool HasRelation(EntityRole role, EntityRelation requiredRelation);

        bool TryQueryRelationTarget(EntityRelation r, out IReadOnlyCollection<Type> result);
    }
}