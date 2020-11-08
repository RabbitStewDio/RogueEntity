using System.Linq;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public readonly struct ModuleEntityRoleInitializerInfo<TGameContext>
    {
        public readonly EntityRole Role;
        public readonly EntityRole[] RequiredRoles;
        public readonly EntityRelation[] RequiredRelations;
        public readonly ModuleEntityRoleInitializerDelegate<TGameContext> Initializer;

        internal ModuleEntityRoleInitializerInfo(EntityRole role,
                                                 ModuleEntityRoleInitializerDelegate<TGameContext> initializer,
                                                 EntityRole[] requiredRoles,
                                                 EntityRelation[] requiredRelations)
        {
            Initializer = initializer;
            Role = role;
            RequiredRoles = requiredRoles;
            RequiredRelations = requiredRelations;
        }

        public ModuleEntityRoleInitializerInfo<TGameContext> WithRequiredRoles(params EntityRole[] roles)
        {
            return new ModuleEntityRoleInitializerInfo<TGameContext>(Role, Initializer, this.RequiredRoles.Concat(roles).ToArray(), RequiredRelations);
        }

        public ModuleEntityRoleInitializerInfo<TGameContext> WithRequiredRelations(params EntityRelation[] relations)
        {
            return new ModuleEntityRoleInitializerInfo<TGameContext>(Role, Initializer, RequiredRoles, RequiredRelations.Concat(relations).ToArray());
        }
    }

    public static class ModuleEntityRoleInitializerInfo
    {
        public static ModuleEntityRoleInitializerInfo<TGameContext> CreateFor<TGameContext>(EntityRole role, ModuleEntityRoleInitializerDelegate<TGameContext> initializer)
        {
            return new ModuleEntityRoleInitializerInfo<TGameContext>(role, initializer, new EntityRole[0], new EntityRelation[0]);
        }
    }
    
}