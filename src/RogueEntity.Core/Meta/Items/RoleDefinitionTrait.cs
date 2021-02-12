using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using System.Collections.Generic;

namespace RogueEntity.Core.Meta.Items
{
    public class RoleDefinitionTrait<TItemId>: IBulkItemTrait<TItemId>, IReferenceItemTrait<TItemId>
        where TItemId : IEntityKey
    {
        readonly ReadOnlyListWrapper<EntityRole> roles;

        public RoleDefinitionTrait(EntityRole role, params EntityRole[] roles)
        {
            var rolesList = new List<EntityRole>();
            rolesList.Add(role);
            rolesList.AddRange(roles);
            this.roles = rolesList;
            
            Id = $"Trait.RoleDefinition.[{role},{string.Join(",", roles)}]";
        }

        public RoleDefinitionTrait(params EntityRole[] roles)
        {
            this.roles = new List<EntityRole>(roles);
            Id = $"Trait.RoleDefinition.[{string.Join(",", roles)}]";
        }

        public ItemTraitId Id { get; }
        public int Priority => 100;
        
        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            foreach (var r in roles)
            {
                yield return r.Instantiate<TItemId>();
            }
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            yield break;
        }

        TItemId IBulkItemTrait<TItemId>.Initialize(IItemDeclaration item, TItemId reference)
        {
            return reference;
        }

        IBulkItemTrait<TItemId> IBulkItemTrait<TItemId>.CreateInstance()
        {
            return this;
        }

        void IReferenceItemTrait<TItemId>.Initialize(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        {
        }

        void IReferenceItemTrait<TItemId>.Apply(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        {
        }

        IReferenceItemTrait<TItemId> IReferenceItemTrait<TItemId>.CreateInstance()
        {
            return this;
        }
    }
}
