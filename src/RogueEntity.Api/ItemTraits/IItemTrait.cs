using System.Collections.Generic;

namespace RogueEntity.Api.ItemTraits
{
    public interface IItemTrait: ITrait
    {
        public IEnumerable<EntityRoleInstance> GetEntityRoles();
        public IEnumerable<EntityRelationInstance> GetEntityRelations();
    }
}
