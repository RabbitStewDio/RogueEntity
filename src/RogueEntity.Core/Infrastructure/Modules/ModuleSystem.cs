using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Modules.Helpers;
using RogueEntity.Core.Infrastructure.Modules.Initializers;
using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public partial class ModuleSystem<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem<TGameContext>>();

        readonly IServiceResolver serviceResolver;
        readonly Dictionary<ModuleId, ModuleRecord<TGameContext>> modulesById;
        readonly ModuleEntitySystemRegistrations<TGameContext> registrations;
        bool initialized;

        public ModuleSystem([NotNull] IServiceResolver serviceResolver)
        {
            modulesById = new Dictionary<ModuleId, ModuleRecord<TGameContext>>();

            this.serviceResolver = serviceResolver ?? throw new ArgumentNullException(nameof(serviceResolver));
            this.registrations = new ModuleEntitySystemRegistrations<TGameContext>();
        }

        public IGameLoopSystemInformation<TGameContext> Initialize(TGameContext context)
        {
            var (moduleInitializer, globalEntityInfo) = InitializeModules();
            return InitializeSystems(moduleInitializer, globalEntityInfo);
        }
        
        (ModuleInitializer<TGameContext> moduleInitializer, GlobalModuleEntityInformation<TGameContext> EntityInformation) InitializeModules()
        {
            if (initialized)
            {
                throw new InvalidOperationException("Initialization can be called only once");
            }

            initialized = true;

            var initPhase = new ModuleSystemPhaseInit<TGameContext>(modulesById);
            var orderedModules = initPhase.CreateModulesSortedByInitOrder();
            Logger.Debug("Processing Modules in order: \n{Modules}", PrintModuleDependencyList(orderedModules));

            // 1. Setup global functions. Those provide global services or configurations etc, and only depend on the context object.
            //    This processes all known modules and is an alternative to using the constructor for initialization.
            //    The modules are guaranteed to be called after their declared dependencies have been called.
            var phase2 = new ModuleSystemPhaseInitModule<TGameContext>(serviceResolver);
            var phase2Result = phase2.PerformModuleInitialization(orderedModules, initPhase);

            // 2. Initialize content. This sets up EntityDeclarations and thus tells the system which roles are used by each entity key
            //    encountered by the code.
            var phase3 = new ModuleSystemPhaseInitContent<TGameContext>(phase2Result, serviceResolver);
            phase3.InitializeModuleContent();

            // 3. Let the content modules declare all their items and game rules.
            var phase4 = new ModuleSystemPhaseDeclareContent<TGameContext>(phase2Result, serviceResolver, modulesById);
            phase4.DeclareItemTypes();

            // 4. Based on the module information gathered, we can now resolve all roles and relations. First, some sorting.
            var phase5 = new ModuleSystemPhaseResolveRoles<TGameContext>(phase2Result);
            phase5.Process(orderedModules);

            // Recompute module dependencies.
            var orderedModulesAfterContent = initPhase.CreateModulesSortedByInitOrder();
            Logger.Debug("After content declaration - Modules in order: \n{Modules}", PrintModuleDependencyList(orderedModulesAfterContent));

            // 5. Now start initializing trait systems for each entity role.
            var phase6 = new ModuleSystemPhaseDeclareRoleSystems<TGameContext>(phase2Result, serviceResolver);
            phase6.InitializeModuleRoles(orderedModules);
            
            var phase7 = new ModuleSystemPhaseDeclareRelationSystems<TGameContext>(phase2Result, serviceResolver);
            phase7.InitializeModuleRelations(orderedModules);

            // 6. One more time to let services connect with each other at a higher level.
            var phase8 = new ModuleSystemPhaseFinalizeRoleSystems<TGameContext>(phase2Result, serviceResolver);
            phase8.InitializeModuleRoles(orderedModules);
            
            var phase9 = new ModuleSystemPhaseFinalizeRelationSystems<TGameContext>(phase2Result, serviceResolver);
            phase9.InitializeModuleRelations(orderedModules);

            return (phase2Result.ModuleInitializer, phase2Result.EntityInformation);
        }



    }
    
    
}