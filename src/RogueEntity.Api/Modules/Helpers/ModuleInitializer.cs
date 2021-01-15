using System;
using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using Serilog;

namespace RogueEntity.Api.Modules.Helpers
{
    public class ModuleInitializer : IModuleInitializer, IModuleInitializationData
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleInitializer>();

        readonly List<GlobalDeclarationRecord> globalSystems;
        readonly Dictionary<Type, object> moduleInitializers;
        readonly Dictionary<Type, Action<IModuleEntityInitializationCallback>> moduleInitializerCallbacks;
        public ModuleId CurrentModuleId { get; set; }

        public ModuleInitializer()
        {
            moduleInitializers = new Dictionary<Type, object>();
            moduleInitializerCallbacks = new Dictionary<Type, Action<IModuleEntityInitializationCallback>>();
            globalSystems = new List<GlobalDeclarationRecord>();
        }

        public IModuleContentContext<TEntityId> DeclareContentContext<TEntityId>()
            where TEntityId : IEntityKey
        {
            if (moduleInitializers.TryGetValue(typeof(TEntityId), out var raw))
            {
                var context = (ModuleEntityContext<TEntityId>)raw;
                context.CurrentModuleId = CurrentModuleId;
                return context;
            }

            var retval = new ModuleEntityContext<TEntityId>(CurrentModuleId);
            moduleInitializers[typeof(TEntityId)] = retval;
            moduleInitializerCallbacks[typeof(TEntityId)] = CallInit<TEntityId>;
            Logger.Debug("Activated entity type {EntityId} via content context creation", typeof(TEntityId));
            return retval;
        }

        public IModuleEntityContext<TEntityId> DeclareEntityContext<TEntityId>()
            where TEntityId : IEntityKey
        {
            if (moduleInitializers.TryGetValue(typeof(TEntityId), out var raw))
            {
                var context = (ModuleEntityContext<TEntityId>)raw;
                context.CurrentModuleId = CurrentModuleId;
                return context;
            }

            var retval = new ModuleEntityContext<TEntityId>(CurrentModuleId);
            moduleInitializers[typeof(TEntityId)] = retval;
            moduleInitializerCallbacks[typeof(TEntityId)] = CallInit<TEntityId>;
            Logger.Debug("Activated entity type {EntityId} via entity system context creation", typeof(TEntityId));
            return retval;
        }

        void CallInit<TEntityId>(IModuleEntityInitializationCallback callback)
            where TEntityId : IEntityKey
        {
            if (moduleInitializers.TryGetValue(typeof(TEntityId), out var context))
            {
                var mc = (ModuleEntityContext<TEntityId>)context;
                callback.PerformInitialization(mc);
            }
        }

        public bool TryGetCallback(Type entityType, out Action<IModuleEntityInitializationCallback> callback)
        {
            return moduleInitializerCallbacks.TryGetValue(entityType, out callback);
        }

        public IEnumerable<(Type entityType, Action<IModuleEntityInitializationCallback> callback)> EntityInitializers => moduleInitializerCallbacks.Select(e => (e.Key, e.Value));

        public void Register(EntitySystemId id, int priority, GlobalSystemRegistrationDelegate entityRegistration)
        {
            globalSystems.Add(new GlobalDeclarationRecord()
            {
                DeclaringModule = CurrentModuleId,
                Id = id,
                Priority = priority,
                SystemRegistration = entityRegistration,
                InsertionOrder = globalSystems.Count
            });
        }

        public IEnumerable<IGlobalSystemDeclaration> GlobalSystems => globalSystems;

        class GlobalDeclarationRecord : IGlobalSystemDeclaration
        {
            public ModuleId DeclaringModule { get; set; }
            public EntitySystemId Id { get; set; }
            public int Priority { get; set; }
            public int InsertionOrder { get; set; }
            public GlobalSystemRegistrationDelegate SystemRegistration { get; set; }
        }
    }
}
