using System;
using System.Collections.Generic;
using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public class ModuleInitializer<TGameContext> : IModuleInitializer<TGameContext>
    {
        readonly Dictionary<Type, object> moduleInitializers;
        public string CurrentModuleId { get; set; }

        public ModuleInitializer()
        {
            moduleInitializers = new Dictionary<Type, object>();
        }


        public IModuleEntityContext<TGameContext, TEntityId> DeclareEntityContext<TEntityId>() where TEntityId : IEntityKey
        {
            if (moduleInitializers.TryGetValue(typeof(TEntityId), out var raw))
            {
                return (IModuleEntityContext<TGameContext, TEntityId>)raw;
            }

            var retval = new ModuleEntityContext<TGameContext, TEntityId>(CurrentModuleId);
            moduleInitializers[typeof(TEntityId)] = retval;
            return retval;
        }
    }
}