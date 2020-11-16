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
    public readonly struct ModuleSystemPhaseInitModuleResult<TGameContext>
    {
        public readonly ModuleInitializer<TGameContext> ModuleInitializer;
        public readonly GlobalModuleEntityInformation<TGameContext> EntityInformation;
        public readonly ReadOnlyListWrapper<ModuleRecord<TGameContext>> OrderedModules;

        public ModuleSystemPhaseInitModuleResult(ModuleInitializer<TGameContext> moduleInitializer,
                                                 GlobalModuleEntityInformation<TGameContext> entityInformation,
                                                 ReadOnlyListWrapper<ModuleRecord<TGameContext>> orderedModules)
        {
            this.ModuleInitializer = moduleInitializer;
            this.EntityInformation = entityInformation;
            this.OrderedModules = orderedModules;
        }
    }

    public class ModuleSystemPhaseInitModule<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem<TGameContext>>();
        readonly IServiceResolver serviceResolver;

        public ModuleSystemPhaseInitModule([NotNull] IServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver ?? throw new ArgumentNullException(nameof(serviceResolver));
        }

        public ModuleSystemPhaseInitModuleResult<TGameContext> PerformModuleInitialization(ReadOnlyListWrapper<ModuleRecord<TGameContext>> orderedModules,
                                                                                           ModuleSystemPhaseInit<TGameContext> initPhase)
        {
            var moduleInitializer = new ModuleInitializer<TGameContext>();
            var globalInformation = new GlobalModuleEntityInformation<TGameContext>();
            while (true)
            {
                InitializeModule(moduleInitializer, globalInformation, orderedModules);
                var newModules = initPhase.CreateModulesSortedByInitOrder();
                
                if (ApiExtensions.EqualsList(orderedModules, newModules))
                {
                    // module initialization done.
                    return new ModuleSystemPhaseInitModuleResult<TGameContext>(moduleInitializer, globalInformation, newModules);
                }
            }
        }

        List<ModuleInitializerDelegate<TGameContext>> CollectModuleInitializers(ModuleBase module)
        {
            var actions = new List<ModuleInitializerDelegate<TGameContext>>();
            foreach (var m in module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var attr = m.GetCustomAttribute<ModuleInitializerAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (m.IsSameAction(typeof(ModuleInitializationParameter).MakeByRefType(), typeof(IModuleInitializer<TGameContext>)))
                {
                    actions.Add((ModuleInitializerDelegate<TGameContext>)Delegate.CreateDelegate(typeof(ModuleInitializerDelegate<TGameContext>), module, m));
                    Logger.Verbose("Found plain module initializer {Method}", m);
                    continue;
                }

                if (!m.IsSameGenericAction(new[] {typeof(TGameContext)},
                                           out var genericMethod, out var errorHint,
                                           typeof(ModuleInitializationParameter).MakeByRefType(), typeof(IModuleInitializer<TGameContext>)))
                {
                    if (!string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(errorHint);
                    }

                    throw new ArgumentException(
                        $"Expected a method with signature 'void XXX(ModuleInitializationParameter by ref, IModuleInitializer<TGameContext>), but found {m} in module {module.Id}");
                }

                Logger.Verbose("Found generic module initializer {Method}", genericMethod);
                actions.Add((ModuleInitializerDelegate<TGameContext>)Delegate.CreateDelegate(typeof(ModuleInitializerDelegate<TGameContext>), module, genericMethod));
            }

            return actions;
        }

        void InitializeModule(ModuleInitializer<TGameContext> initializer,
                              GlobalModuleEntityInformation<TGameContext> globalInformation,
                              ReadOnlyListWrapper<ModuleRecord<TGameContext>> orderedModules)
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