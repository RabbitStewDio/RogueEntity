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
        public readonly string SourceHint;

        internal ModuleEntityRelationInitializerInfo(EntityRelation role,
                                                     ModuleEntityRelationInitializerDelegate<TGameContext, TEntityId> initializer,
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

        public ModuleEntityRelationInitializerInfo<TGameContext, TEntityId> WithRequiredSubjectRoles(params EntityRole[] roles)
        {
            return new ModuleEntityRelationInitializerInfo<TGameContext, TEntityId>(Relation, Initializer, this.RequiredSubjectRoles.Concat(roles).ToArray(), RequiredObjectRoles, SourceHint);
        }

        public ModuleEntityRelationInitializerInfo<TGameContext, TEntityId> WithRequiredTargetRoles(params EntityRole[] relations)
        {
            return new ModuleEntityRelationInitializerInfo<TGameContext, TEntityId>(Relation, Initializer, RequiredSubjectRoles, RequiredObjectRoles.Concat(relations).ToArray(), SourceHint);
        }
    }

    public static class ModuleEntityRelationInitializerInfo
    {
        public static ModuleEntityRelationInitializerInfo<TGameContext, TEntityId> CreateFor<TGameContext, TEntityId>(EntityRelation role,
                                                                                                                      ModuleEntityRelationInitializerDelegate<TGameContext, TEntityId> initializer)
            where TEntityId : IEntityKey
        {
            var sourceHint = initializer.Target.GetType() + "#" + initializer.Method.Name;
            return new ModuleEntityRelationInitializerInfo<TGameContext, TEntityId>(role, initializer, new EntityRole[0], new EntityRole[0], sourceHint);
        }
        
        public static ModuleEntityRelationInitializerInfo<TGameContext, TEntityId> CreateFor<TGameContext, TEntityId>(EntityRelation role,
                                                                                                                      ModuleEntityRelationInitializerDelegate<TGameContext, TEntityId> initializer,
                                                                                                                      string sourceHint)
            where TEntityId : IEntityKey
        {
            return new ModuleEntityRelationInitializerInfo<TGameContext, TEntityId>(role, initializer, new EntityRole[0], new EntityRole[0], sourceHint);
        }
    }
}