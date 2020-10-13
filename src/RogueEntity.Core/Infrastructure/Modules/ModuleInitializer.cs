using System;
using System.Collections.Generic;
using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public class ModuleInitializer<TGameContext> : IModuleInitializer<TGameContext>
    {
        readonly List<GlobalRegistrationRecord> globalSystems;
        readonly Dictionary<Type, object> moduleInitializers;
        public string CurrentModuleId { get; set; }

        public ModuleInitializer()
        {
            moduleInitializers = new Dictionary<Type, object>();
            globalSystems = new List<GlobalRegistrationRecord>();
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

        public void Register(EntitySystemId id, int priority, ModuleEntityContext.GlobalSystemRegistrationDelegate<TGameContext> entityRegistration)
        {
            globalSystems.Add(new GlobalRegistrationRecord()
            {
                DeclaringModule = CurrentModuleId,
                Id = id,
                Priority = priority,
                SystemRegistration = entityRegistration
            });
        }

        class GlobalRegistrationRecord
        {
            public string DeclaringModule { get; set; }
            public EntitySystemId Id { get; set; }
            public int Priority { get; set; }
            public ModuleEntityContext.GlobalSystemRegistrationDelegate<TGameContext> SystemRegistration;
        }
    }
}