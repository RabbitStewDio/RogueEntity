using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;
using RogueEntity.Api.Utils;
using Serilog;

namespace RogueEntity.Api.Modules.Initializers
{
    public readonly struct ModuleSystemPhaseInitModuleResult
    {
        public readonly ModuleInitializer ModuleInitializer;
        public readonly GlobalModuleEntityInformation EntityInformation;
        public readonly ReadOnlyListWrapper<ModuleRecord> OrderedModules;

        public ModuleSystemPhaseInitModuleResult(ModuleInitializer moduleInitializer,
                                                 GlobalModuleEntityInformation entityInformation,
                                                 ReadOnlyListWrapper<ModuleRecord> orderedModules)
        {
            this.ModuleInitializer = moduleInitializer;
            this.EntityInformation = entityInformation;
            this.OrderedModules = orderedModules;
        }
    }

    public class ModuleSystemPhaseInitModule
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem>();
        readonly IServiceResolver serviceResolver;

        public ModuleSystemPhaseInitModule([NotNull] IServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver ?? throw new ArgumentNullException(nameof(serviceResolver));
        }

        public ModuleSystemPhaseInitModuleResult PerformModuleInitialization(ReadOnlyListWrapper<ModuleRecord> orderedModules,
                                                                             ModuleSystemPhaseInit initPhase)
        {
            var moduleInitializer = new ModuleInitializer();
            var globalInformation = new GlobalModuleEntityInformation();
            var circuitBreaker = 100;
            while (circuitBreaker > 0)
            {
                InitializeModule(moduleInitializer, globalInformation, orderedModules);
                var newModules = initPhase.CreateModulesSortedByInitOrder();

                if (ApiExtensions.EqualsList(orderedModules, newModules))
                {
                    // module initialization done.
                    return new ModuleSystemPhaseInitModuleResult(moduleInitializer, globalInformation, newModules);
                }

                circuitBreaker -= 1;
            }

            throw new ModuleInitializationException("Did not reach a stable state after 100 iterations of module search.");
        }

        List<ModuleInitializerDelegate> CollectModuleInitializers(IModule module)
        {
            var actions = new List<ModuleInitializerDelegate>();
            foreach (var m in module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var attr = m.GetCustomAttribute<ModuleInitializerAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (m.IsSameAction(typeof(ModuleInitializationParameter).MakeByRefType(), typeof(IModuleInitializer)))
                {
                    var moduleInitializerDelegate = (ModuleInitializerDelegate)Delegate.CreateDelegate(typeof(ModuleInitializerDelegate), module, m);
                    actions.Add(moduleInitializerDelegate);
                    Logger.Verbose("Found plain module initializer {Method}", m.Name);
                    continue;
                }

                throw new ArgumentException(
                    $"Expected a method with signature 'void XXX(ModuleInitializationParameter by ref, IModuleInitializer), but found {m} in module {module.Id}");
            }

            return actions;
        }

        void InitializeModule(ModuleInitializer initializer,
                              GlobalModuleEntityInformation globalInformation,
                              ReadOnlyListWrapper<ModuleRecord> orderedModules)
        {
            var mip = new ModuleInitializationParameter(globalInformation, serviceResolver);
            foreach (var mod in orderedModules)
            {
                if (mod.InitializedModule)
                {
                    continue;
                }

                try
                {
                    initializer.CurrentModuleId = mod.ModuleId;
                    mod.InitializedModule = true;
                    var initializers = CollectModuleInitializers(mod.Module);
                    foreach (var mi in initializers)
                    {
                        mi(in mip, initializer);
                    }
                }
                finally
                {
                    initializer.CurrentModuleId = null;
                }
            }
        }
    }
}
