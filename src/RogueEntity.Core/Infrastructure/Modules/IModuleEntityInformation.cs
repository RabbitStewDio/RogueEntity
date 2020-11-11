using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public interface IModuleEntityInformation
    {
        public bool HasRole(EntityRole role);
        
        public bool HasRole(EntityRole role, EntityRole requiredRole);
        public bool HasRelation(EntityRole role, EntityRelation requiredRelation);
    }
}