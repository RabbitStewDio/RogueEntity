using System.Linq;
using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public readonly struct ModuleEntityRelationInitializerInfo<TGameContext>
    {
        public readonly EntityRelation Relation;
        public readonly EntityRole[] RequiredSubjectRoles;
        public readonly EntityRole[] RequiredObjectRoles;
        public readonly ModuleEntityRelationInitializerDelegate<TGameContext> Initializer;

        internal ModuleEntityRelationInitializerInfo(EntityRelation role,
                                                     ModuleEntityRelationInitializerDelegate<TGameContext> initializer,
                                                     EntityRole[] requiredSubjectRoles,
                                                     EntityRole[] requiredObjectRoles)
        {
            Initializer = initializer;
            Relation = role;
            RequiredSubjectRoles = requiredSubjectRoles;
            RequiredObjectRoles = requiredObjectRoles;
        }

        public ModuleEntityRelationInitializerInfo<TGameContext> WithRequiredSubjectRoles(params EntityRole[] roles)
        {
            return new ModuleEntityRelationInitializerInfo<TGameContext>(Relation, Initializer, this.RequiredSubjectRoles.Concat(roles).ToArray(), RequiredObjectRoles);
        }

        public ModuleEntityRelationInitializerInfo<TGameContext> WithRequiredTargetRoles(params EntityRole[] relations)
        {
            return new ModuleEntityRelationInitializerInfo<TGameContext>(Relation, Initializer, RequiredSubjectRoles, RequiredObjectRoles.Concat(relations).ToArray());
        }
    }

    public static class ModuleEntityRelationInitializerInfo
    {
        public static ModuleEntityRelationInitializerInfo<TGameContext> CreateFor<TGameContext>(EntityRelation role, ModuleEntityRelationInitializerDelegate<TGameContext> initializer)
        {
            return new ModuleEntityRelationInitializerInfo<TGameContext>(role, initializer, new EntityRole[0], new EntityRole[0]);
        }
    }
}