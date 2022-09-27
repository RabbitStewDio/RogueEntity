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
        static readonly ILogger logger = SLog.ForContext<ModuleInitializer>();

        readonly List<GlobalDeclarationRecord> globalSystems;
        readonly List<GlobalDeclarationRecord> globalFinalizerSystems;
        readonly Dictionary<Type, object> moduleInitializers;
        readonly Dictionary<Type, Action<IModuleEntityInitializationCallback>> moduleInitializerCallbacks;
        public ModuleId CurrentModuleId { get; set; }

        public ModuleInitializer()
        {
            moduleInitializers = new Dictionary<Type, object>();
            moduleInitializerCallbacks = new Dictionary<Type, Action<IModuleEntityInitializationCallback>>();
            globalSystems = new List<GlobalDeclarationRecord>();
            globalFinalizerSystems = new List<GlobalDeclarationRecord>();
        }

        public IModuleContentContext<TEntityId> DeclareContentContext<TEntityId>()
            where TEntityId : struct, IEntityKey
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
            logger.Debug("Activated entity type {EntityId} via content context creation", typeof(TEntityId));
            return retval;
        }

        public IModuleEntityContext<TEntityId> DeclareEntityContext<TEntityId>()
            where TEntityId : struct, IEntityKey
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
            logger.Debug("Activated entity type {EntityId} via entity system context creation", typeof(TEntityId));
            return retval;
        }

        void CallInit<TEntityId>(IModuleEntityInitializationCallback callback)
            where TEntityId : struct, IEntityKey
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
            globalSystems.Add(new GlobalDeclarationRecord(entityRegistration)
            {
                DeclaringModule = CurrentModuleId,
                Id = id,
                Priority = priority,
                InsertionOrder = globalSystems.Count
            });
        }

        public void RegisterFinalizer(EntitySystemId id, int priority, GlobalSystemRegistrationDelegate entityRegistration)
        {
            globalFinalizerSystems.Add(new GlobalDeclarationRecord(entityRegistration)
            {
                DeclaringModule = CurrentModuleId,
                Id = id,
                Priority = priority,
                InsertionOrder = globalFinalizerSystems.Count
            });
        }

        public IEnumerable<IGlobalSystemDeclaration> GlobalSystems => globalSystems;
        public IEnumerable<IGlobalSystemDeclaration> GlobalFinalizerSystems => globalFinalizerSystems;

        class GlobalDeclarationRecord : IGlobalSystemDeclaration
        {
            public GlobalDeclarationRecord(GlobalSystemRegistrationDelegate systemRegistration)
            {
                SystemRegistration = systemRegistration;
            }

            public ModuleId DeclaringModule { get; set; }
            public EntitySystemId Id { get; set; }
            public int Priority { get; set; }
            public int InsertionOrder { get; set; }
            public GlobalSystemRegistrationDelegate SystemRegistration { get; }
        }
    }
}
