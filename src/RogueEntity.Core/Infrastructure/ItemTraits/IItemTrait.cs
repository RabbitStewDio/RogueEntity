using System.Collections.Generic;

namespace RogueEntity.Core.Infrastructure.ItemTraits
{
    public interface IItemTrait: ITrait
    {
        public IEnumerable<EntityRoleInstance> GetEntityRoles();
        public IEnumerable<EntityRelationInstance> GetEntityRelations();
    }
}
