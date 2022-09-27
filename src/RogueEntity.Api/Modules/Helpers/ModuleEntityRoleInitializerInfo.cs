using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using System;

namespace RogueEntity.Api.Modules.Helpers
{
    public readonly struct ModuleEntityRoleInitializerInfo<TEntityId>
        where TEntityId : struct, IEntityKey
    {
        public readonly EntityRole Role;
        public readonly EntityRole[] RequiredRoles;
        public readonly EntityRole[] RequiredRolesAnywhereInSystem;
        public readonly EntityRelation[] RequiredRelations;
        public readonly ModuleEntityRoleInitializerDelegate<TEntityId> Initializer;
        public readonly string SourceHint;

        internal ModuleEntityRoleInitializerInfo(EntityRole role,
                                                 ModuleEntityRoleInitializerDelegate<TEntityId> initializer,
                                                 EntityRole[] requiredRoles,
                                                 EntityRelation[] requiredRelations,
                                                 EntityRole[] requiredRolesAnywhereInSystem,
                                                 string sourceHint)
        {
            Initializer = initializer;
            SourceHint = sourceHint;
            Role = role;
            RequiredRoles = requiredRoles ?? Array.Empty<EntityRole>();
            RequiredRelations = requiredRelations ?? Array.Empty<EntityRelation>();
            RequiredRolesAnywhereInSystem = requiredRolesAnywhereInSystem ?? Array.Empty<EntityRole>();
        }

        public ModuleEntityRoleInitializerInfo<TEntityId> WithRequiredRolesAnywhereInSystem(params EntityRole[] roles)
        {
            return new ModuleEntityRoleInitializerInfo<TEntityId>(Role, Initializer, this.RequiredRoles, RequiredRelations, roles.Concat(roles).ToArray(), SourceHint);
        }

        public ModuleEntityRoleInitializerInfo<TEntityId> WithRequiredRoles(params EntityRole[] roles)
        {
            return new ModuleEntityRoleInitializerInfo<TEntityId>(Role, Initializer, this.RequiredRoles.Concat(roles).ToArray(), RequiredRelations, this.RequiredRolesAnywhereInSystem, SourceHint);
        }

        public ModuleEntityRoleInitializerInfo<TEntityId> WithRequiredRelations(params EntityRelation[] relations)
        {
            return new ModuleEntityRoleInitializerInfo<TEntityId>(Role, Initializer, RequiredRoles, RequiredRelations.Concat(relations).ToArray(), this.RequiredRolesAnywhereInSystem, SourceHint);
        }
    }

    public static class ModuleEntityRoleInitializerInfo
    {
        public static ModuleEntityRoleInitializerInfo<TEntityId> CreateFor<TEntityId>(EntityRole role,
                                                                                      ModuleEntityRoleInitializerDelegate<TEntityId> initializer,
                                                                                      string sourceHint)
            where TEntityId : struct, IEntityKey
        {
            return new ModuleEntityRoleInitializerInfo<TEntityId>(role, initializer, Array.Empty<EntityRole>(), Array.Empty<EntityRelation>(), Array.Empty<EntityRole>(), sourceHint);
        }

        public static ModuleEntityRoleInitializerInfo<TEntityId> CreateFor<TEntityId>(EntityRole role, ModuleEntityRoleInitializerDelegate<TEntityId> initializer)
            where TEntityId : struct, IEntityKey
        {
            var sourceHint = initializer.Target.GetType() + "#" + initializer.Method.Name;
            return new ModuleEntityRoleInitializerInfo<TEntityId>(role, initializer, Array.Empty<EntityRole>(), Array.Empty<EntityRelation>(), Array.Empty<EntityRole>(), sourceHint);
        }
    }
}
