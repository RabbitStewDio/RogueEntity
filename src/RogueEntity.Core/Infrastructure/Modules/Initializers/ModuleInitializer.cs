﻿using System;
using System.Collections.Generic;
using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Modules.Initializers
{
    public class ModuleInitializer<TGameContext> : IModuleInitializer<TGameContext>, IModuleInitializationData<TGameContext>
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

        public void Register(EntitySystemId id, int priority, GlobalSystemRegistrationDelegate<TGameContext> entityRegistration)
        {
            globalSystems.Add(new GlobalRegistrationRecord()
            {
                DeclaringModule = CurrentModuleId,
                Id = id,
                Priority = priority,
                SystemRegistration = entityRegistration,
                InsertionOrder = globalSystems.Count
            });
        }

        public IEnumerable<IGlobalSystemRegistration<TGameContext>> GlobalSystems => globalSystems;

        class GlobalRegistrationRecord: IGlobalSystemRegistration<TGameContext>
        {
            public string DeclaringModule { get; set; }
            public EntitySystemId Id { get; set; }
            public int Priority { get; set; }
            public int InsertionOrder { get; set; }
            public GlobalSystemRegistrationDelegate<TGameContext> SystemRegistration { get; set; }
        }
    }
}