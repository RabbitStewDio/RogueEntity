using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Api.Modules.Helpers
{
    public readonly struct ModuleEntityRelationInitializerInfo<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        public readonly EntityRelation Relation;
        public readonly EntityRole[] RequiredSubjectRoles;
        public readonly EntityRole[] RequiredObjectRoles;
        public readonly ModuleEntityRelationInitializerDelegate<TGameContext, TEntityId> Initializer;

        internal ModuleEntityRelationInitializerInfo(EntityRelation role,
                                                     ModuleEntityRelationInitializerDelegate<TGameContext, TEntityId> initializer,
                                                     EntityRole[] requiredSubjectRoles,
                                                     EntityRole[] requiredObjectRoles)
        {
            Initializer = initializer;
            Relation = role;
            RequiredSubjectRoles = requiredSubjectRoles;
            RequiredObjectRoles = requiredObjectRoles;
        }

        public ModuleEntityRelationInitializerInfo<TGameContext, TEntityId> WithRequiredSubjectRoles(params EntityRole[] roles)
        {
            return new ModuleEntityRelationInitializerInfo<TGameContext, TEntityId>(Relation, Initializer, this.RequiredSubjectRoles.Concat(roles).ToArray(), RequiredObjectRoles);
        }

        public ModuleEntityRelationInitializerInfo<TGameContext, TEntityId> WithRequiredTargetRoles(params EntityRole[] relations)
        {
            return new ModuleEntityRelationInitializerInfo<TGameContext, TEntityId>(Relation, Initializer, RequiredSubjectRoles, RequiredObjectRoles.Concat(relations).ToArray());
        }
    }

    public static class ModuleEntityRelationInitializerInfo
    {
        public static ModuleEntityRelationInitializerInfo<TGameContext, TEntityId> CreateFor<TGameContext, TEntityId>(EntityRelation role,
                                                                                                                      ModuleEntityRelationInitializerDelegate<TGameContext, TEntityId> initializer)
            where TEntityId : IEntityKey
        {
            return new ModuleEntityRelationInitializerInfo<TGameContext, TEntityId>(role, initializer, new EntityRole[0], new EntityRole[0]);
        }
    }
}