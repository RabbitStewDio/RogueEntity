﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Modules.Initializers;
using RogueEntity.Api.Services;
using RogueEntity.Api.Utils;
using Serilog;

namespace RogueEntity.Api.Modules
{
    public partial class ModuleSystem
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem>();

        readonly IServiceResolver serviceResolver;
        readonly Dictionary<ModuleId, ModuleRecord> modulesById;
        readonly ModuleEntitySystemRegistrations registrations;
        bool initialized;

        public ModuleSystem(IServiceResolver serviceResolver)
        {
            modulesById = new Dictionary<ModuleId, ModuleRecord>();

            this.serviceResolver = serviceResolver ?? throw new ArgumentNullException(nameof(serviceResolver));
            this.registrations = new ModuleEntitySystemRegistrations();

            if (!this.serviceResolver.TryResolve<IConfigurationHost>(out var config))
            {
                config = new DefaultConfigurationHost(new ConfigurationBuilder().Build());
                this.serviceResolver.Store(config);
            }
        }

        public IGameLoopSystemInformation Initialize()
        {
            var (moduleInitializer, globalEntityInfo) = InitializeModules();
            return InitializeGlobalSystems(moduleInitializer, globalEntityInfo);
        }
        
        (ModuleInitializer moduleInitializer, GlobalModuleEntityInformation EntityInformation) InitializeModules()
        {
            if (initialized)
            {
                throw new InvalidOperationException("Initialization can be called only once");
            }

            initialized = true;

            var initPhase = new ModuleSystemPhaseInit(modulesById);
            var orderedModules = initPhase.CreateModulesSortedByInitOrder();
            Logger.Debug("Processing Modules in order: \n{Modules}", PrintModuleDependencyList(orderedModules));

            // 1. Setup global functions. Those provide global services or configurations etc, and only depend on the context object.
            //    This processes all known modules and is an alternative to using the constructor for initialization.
            //    The modules are guaranteed to be called after their declared dependencies have been called.
            var phase2 = new ModuleSystemPhaseInitModule(serviceResolver);
            var phase2Result = phase2.PerformModuleInitialization(orderedModules, initPhase);

            // 2. Initialize content. This sets up EntityDeclarations and thus tells the system which roles are used by each entity key
            //    encountered by the code.
            var phase3a = new ModuleSystemPhaseInitContent(phase2Result, serviceResolver);
            phase3a.InitializeModuleContent();

            var phase3b = new ModuleSystemPhaseInitLateModule(phase2Result, serviceResolver);
            phase3b.InitializeModuleContent();

            // 3. Let the content modules declare all their items and game rules.
            var phase4 = new ModuleSystemPhaseDeclareContent(phase2Result, serviceResolver, modulesById);
            phase4.DeclareItemTypes();

            // 4. Based on the module information gathered, we can now resolve all roles and relations. First, some sorting.
            var phase5 = new ModuleSystemPhaseResolveRoles(phase2Result);
            phase5.Process(orderedModules);

            // Recompute module dependencies.
            var orderedModulesAfterContent = initPhase.CreateModulesSortedByInitOrder();
            Logger.Debug("After content declaration - Modules in order: \n{Modules}", PrintModuleDependencyList(orderedModulesAfterContent));
            Logger.Debug("Entity Roles: \n{Roles}", string.Join(",\n", phase2Result.EntityInformation.Roles));
            Logger.Debug("Entity Relations: \n{Relations}", string.Join(",\n", phase2Result.EntityInformation.Relations));

            // 5. Now start initializing trait systems for each entity role.
            var phase6 = new ModuleSystemPhaseDeclareRoleSystems(phase2Result, serviceResolver);
            phase6.InitializeModuleRoles(orderedModules);
            
            var phase7 = new ModuleSystemPhaseDeclareRelationSystems(phase2Result, serviceResolver);
            phase7.InitializeModuleRelations(orderedModules);

            // 6. One more time to let services connect with each other at a higher level.
            var phase8 = new ModuleSystemPhaseFinalizeRoleSystems(phase2Result, serviceResolver);
            phase8.InitializeModuleRoles(orderedModules);
            
            var phase9 = new ModuleSystemPhaseFinalizeRelationSystems(phase2Result, serviceResolver);
            phase9.InitializeModuleRelations(orderedModules);

            var phase10 = new ModuleSystemPhaseFinalizeModule(phase2Result, serviceResolver);
            phase10.InitializeModuleContent();
            
            return (phase2Result.ModuleInitializer, phase2Result.EntityInformation);
        }



    }
    
    
}