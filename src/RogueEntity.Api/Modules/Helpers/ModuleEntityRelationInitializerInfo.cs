using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Api.Modules.Helpers
{
    public readonly struct ModuleEntityRelationInitializerInfo<TEntityId>
        where TEntityId : IEntityKey
    {
        public readonly EntityRelation Relation;
        public readonly EntityRole[] RequiredSubjectRoles;
        public readonly EntityRole[] RequiredObjectRoles;
        public readonly ModuleEntityRelationInitializerDelegate<TEntityId> Initializer;
        public readonly string SourceHint;

        internal ModuleEntityRelationInitializerInfo(EntityRelation role,
                                                     ModuleEntityRelationInitializerDelegate<TEntityId> initializer,
                                                     EntityRole[] requiredSubjectRoles,
                                                     EntityRole[] requiredObjectRoles,
                                                     string sourceHint)
        {
            Initializer = initializer;
            Relation = role;
            RequiredSubjectRoles = requiredSubjectRoles;
            RequiredObjectRoles = requiredObjectRoles;
            SourceHint = sourceHint;
        }

        public ModuleEntityRelationInitializerInfo<TEntityId> WithRequiredSubjectRoles(params EntityRole[] roles)
        {
            return new ModuleEntityRelationInitializerInfo<TEntityId>(Relation, Initializer, this.RequiredSubjectRoles.Concat(roles).ToArray(), RequiredObjectRoles, SourceHint);
        }

        public ModuleEntityRelationInitializerInfo<TEntityId> WithRequiredTargetRoles(params EntityRole[] relations)
        {
            return new ModuleEntityRelationInitializerInfo<TEntityId>(Relation, Initializer, RequiredSubjectRoles, RequiredObjectRoles.Concat(relations).ToArray(), SourceHint);
        }
    }

    public static class ModuleEntityRelationInitializerInfo
    {
        public static ModuleEntityRelationInitializerInfo<TEntityId> CreateFor<TEntityId>(EntityRelation role,
                                                                                          ModuleEntityRelationInitializerDelegate<TEntityId> initializer)
            where TEntityId : IEntityKey
        {
            var sourceHint = initializer.Target.GetType() + "#" + initializer.Method.Name;
            return new ModuleEntityRelationInitializerInfo<TEntityId>(role, initializer, new EntityRole[0], new EntityRole[0], sourceHint);
        }

        public static ModuleEntityRelationInitializerInfo<TEntityId> CreateFor<TEntityId>(EntityRelation role,
                                                                                          ModuleEntityRelationInitializerDelegate<TEntityId> initializer,
                                                                                          string sourceHint)
            where TEntityId : IEntityKey
        {
            return new ModuleEntityRelationInitializerInfo<TEntityId>(role, initializer, new EntityRole[0], new EntityRole[0], sourceHint);
        }
    }
}
