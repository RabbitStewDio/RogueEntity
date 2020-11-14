using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Infrastructure.Modules.Helpers
{
    public readonly struct ModuleEntityRoleInitializerInfo<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        public readonly EntityRole Role;
        public readonly EntityRole[] RequiredRoles;
        public readonly EntityRole[] RequiredRolesAnywhereInSystem;
        public readonly EntityRelation[] RequiredRelations;
        public readonly ModuleEntityRoleInitializerDelegate<TGameContext, TEntityId> Initializer;
        public readonly string SourceHint;

        internal ModuleEntityRoleInitializerInfo(EntityRole role,
                                                 ModuleEntityRoleInitializerDelegate<TGameContext, TEntityId> initializer,
                                                 EntityRole[] requiredRoles,
                                                 EntityRelation[] requiredRelations,
                                                 EntityRole[] requiredRolesAnywhereInSystem,
                                                 string sourceHint)
        {
            Initializer = initializer;
            SourceHint = sourceHint;
            Role = role;
            RequiredRoles = requiredRoles ?? new EntityRole[0];
            RequiredRelations = requiredRelations ?? new EntityRelation[0];
            RequiredRolesAnywhereInSystem = requiredRolesAnywhereInSystem ?? new EntityRole[0];
        }

        public ModuleEntityRoleInitializerInfo<TGameContext, TEntityId> WithRequiredRolesAnywhereInSystem(params EntityRole[] roles)
        {
            return new ModuleEntityRoleInitializerInfo<TGameContext, TEntityId>(Role, Initializer, this.RequiredRoles, RequiredRelations, roles.Concat(roles).ToArray(), SourceHint);
        }

        public ModuleEntityRoleInitializerInfo<TGameContext, TEntityId> WithRequiredRoles(params EntityRole[] roles)
        {
            return new ModuleEntityRoleInitializerInfo<TGameContext, TEntityId>(Role, Initializer, this.RequiredRoles.Concat(roles).ToArray(), RequiredRelations, this.RequiredRolesAnywhereInSystem, SourceHint);
        }

        public ModuleEntityRoleInitializerInfo<TGameContext, TEntityId> WithRequiredRelations(params EntityRelation[] relations)
        {
            return new ModuleEntityRoleInitializerInfo<TGameContext, TEntityId>(Role, Initializer, RequiredRoles, RequiredRelations.Concat(relations).ToArray(), this.RequiredRolesAnywhereInSystem, SourceHint);
        }
    }

    public static class ModuleEntityRoleInitializerInfo
    {
        public static ModuleEntityRoleInitializerInfo<TGameContext, TEntityId> CreateFor<TGameContext, TEntityId>(EntityRole role, ModuleEntityRoleInitializerDelegate<TGameContext, TEntityId> initializer,
                                                                                                                  string sourceHint)
            where TEntityId : IEntityKey
        {
            return new ModuleEntityRoleInitializerInfo<TGameContext, TEntityId>(role, initializer, new EntityRole[0], new EntityRelation[0], new EntityRole[0], sourceHint);
        }
        
        public static ModuleEntityRoleInitializerInfo<TGameContext, TEntityId> CreateFor<TGameContext, TEntityId>(EntityRole role, ModuleEntityRoleInitializerDelegate<TGameContext, TEntityId> initializer)
            where TEntityId : IEntityKey
        {
            var sourceHint = initializer.Target.GetType() + "#" + initializer.Method.Name;
            return new ModuleEntityRoleInitializerInfo<TGameContext, TEntityId>(role, initializer, new EntityRole[0], new EntityRelation[0], new EntityRole[0], sourceHint);
        }
    }
    
}