using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using System.Collections.Generic;

namespace RogueEntity.Api.Modules
{
    public interface IModule
    {
        ModuleId Id { get; }
        ReadOnlyListWrapper<EntityRelation> RequiredRelations { get; }
        ReadOnlyListWrapper<ModuleDependency> ModuleDependencies { get; }
        IEnumerable<DeclaredEntityRoleRecord> DeclaredEntityTypes { get; }
        IEnumerable<DeclaredEntityRelationRecord> DeclaredEntityRelations { get; }
        
        IEnumerable<EntityRole> QueryDeclaredRolesForEntityType<TEntityType>();
        
        bool HasRequiredRole(in EntityRole role);
        bool HasRequiredRelation(in EntityRelation relation);
    }
}
