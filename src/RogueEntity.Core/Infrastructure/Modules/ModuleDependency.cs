using System;
using EnTTSharp.Entities;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public readonly struct ModuleDependency
    {
        public readonly bool EntityModule;
        public readonly string ModuleId;
        public readonly Optional<Type> EntityType;

        ModuleDependency(string moduleId, bool entityModule, Optional<Type> entityType = default)
        {
            ModuleId = moduleId;
            EntityType = entityType;
            EntityModule = entityModule;
        }

        public static ModuleDependency OfContent(string moduleId)
        {
            return new ModuleDependency(moduleId, false);
        }



        public static ModuleDependency OfFrameworkEntity(string moduleId)
        {
            return new ModuleDependency(moduleId, true);
        }

        public static ModuleDependency OfEntity<TEntity>(string moduleId) where TEntity: IEntityKey
        {
            return new ModuleDependency(moduleId, true, typeof(TEntity));
        }
    }


}