using System;
using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Modules.Helpers
{
    public class ModuleInitializer<TGameContext> : IModuleInitializer<TGameContext>, IModuleInitializationData<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleInitializer<TGameContext>>();
        
        readonly List<GlobalDeclarationRecord> globalSystems;
        readonly Dictionary<Type, object> moduleInitializers;
        readonly Dictionary<Type, Action<IModuleEntityInitializationCallback<TGameContext>>> moduleInitializerCallbacks;
        public ModuleId CurrentModuleId { get; set; }

        public ModuleInitializer()
        {
            moduleInitializers = new Dictionary<Type, object>();
            moduleInitializerCallbacks = new Dictionary<Type, Action<IModuleEntityInitializationCallback<TGameContext>>>();
            globalSystems = new List<GlobalDeclarationRecord>();
        }

        public IModuleContentContext<TGameContext, TEntityId> DeclareContentContext<TEntityId>()
            where TEntityId : IEntityKey
        {
            if (moduleInitializers.TryGetValue(typeof(TEntityId), out var raw))
            {
                var context = (ModuleEntityContext<TGameContext, TEntityId>)raw;
                context.CurrentModuleId = CurrentModuleId;
                return context;
            }

            var retval = new ModuleEntityContext<TGameContext, TEntityId>(CurrentModuleId);
            moduleInitializers[typeof(TEntityId)] = retval;
            moduleInitializerCallbacks[typeof(TEntityId)] = CallInit<TEntityId>;
            Logger.Debug("Activated entity type {EntityId} via content context creation", typeof(TEntityId));
            return retval;
        }

        public IModuleEntityContext<TGameContext, TEntityId> DeclareEntityContext<TEntityId>()
            where TEntityId : IEntityKey
        {
            if (moduleInitializers.TryGetValue(typeof(TEntityId), out var raw))
            {
                var context = (ModuleEntityContext<TGameContext, TEntityId>)raw;
                context.CurrentModuleId = CurrentModuleId;
                return context;
            }

            var retval = new ModuleEntityContext<TGameContext, TEntityId>(CurrentModuleId);
            moduleInitializers[typeof(TEntityId)] = retval;
            moduleInitializerCallbacks[typeof(TEntityId)] = CallInit<TEntityId>;
            Logger.Debug("Activated entity type {EntityId} via entity system context creation", typeof(TEntityId));
            return retval;
        }

        void CallInit<TEntityId>(IModuleEntityInitializationCallback<TGameContext> callback)
            where TEntityId : IEntityKey
        {
            if (moduleInitializers.TryGetValue(typeof(TEntityId), out var context))
            {
                var mc = (ModuleEntityContext<TGameContext, TEntityId>)context;
                callback.PerformInitialization(mc);
            }
        }

        public bool TryGetCallback(Type entityType, out Action<IModuleEntityInitializationCallback<TGameContext>> callback)
        {
            return moduleInitializerCallbacks.TryGetValue(entityType, out callback);
        }
        
        public IEnumerable<(Type entityType, Action<IModuleEntityInitializationCallback<TGameContext>> callback)> EntityInitializers => moduleInitializerCallbacks.Select(e => (e.Key, e.Value));
        
        public void Register(EntitySystemId id, int priority, GlobalSystemRegistrationDelegate<TGameContext> entityRegistration)
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

        public IEnumerable<IGlobalSystemDeclaration<TGameContext>> GlobalSystems => globalSystems;

        class GlobalDeclarationRecord : IGlobalSystemDeclaration<TGameContext>
        {
            public ModuleId DeclaringModule { get; set; }
            public EntitySystemId Id { get; set; }
            public int Priority { get; set; }
            public int InsertionOrder { get; set; }
            public GlobalSystemRegistrationDelegate<TGameContext> SystemRegistration { get; set; }
        }
    }
}