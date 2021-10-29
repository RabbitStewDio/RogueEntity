using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;
using RogueEntity.Api.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RogueEntity.Api.Modules.Initializers
{
    public class ModuleSystemPhaseFinalizeModule
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem>();

        readonly IServiceResolver serviceResolver;
        readonly GlobalModuleEntityInformation moduleInfo;
        readonly ModuleInitializer initializer;
        readonly ReadOnlyListWrapper<ModuleRecord> orderedModules;

        public ModuleSystemPhaseFinalizeModule(ModuleSystemPhaseInitModuleResult previousResult,
                                               IServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver;
            this.moduleInfo = previousResult.EntityInformation;
            this.initializer = previousResult.ModuleInitializer;
            this.orderedModules = previousResult.OrderedModules;
        }
        
        public void InitializeModuleContent()
        {
            var mip = new ModuleInitializationParameter(moduleInfo, serviceResolver);
            foreach (var mod in orderedModules)
            {
                if (mod.FinalizedModule)
                {
                    continue;
                }

                try
                {
                    initializer.CurrentModuleId = mod.ModuleId;

                    mod.FinalizedModule = true;
                    var contentInitializers = CollectModuleFinalizers(mod);
                    foreach (var mi in contentInitializers)
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

        static List<ModuleContentInitializerDelegate> CollectModuleFinalizers(ModuleRecord module)
        {
            var actions = new List<ModuleContentInitializerDelegate>();
            foreach (var m in module.ModuleMethods)
            {
                var attr = m.GetCustomAttribute<ModuleFinalizerAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (m.IsSameAction(typeof(ModuleInitializationParameter).MakeByRefType(), typeof(IModuleInitializer)))
                {
                    actions.Add((ModuleContentInitializerDelegate)Delegate.CreateDelegate(typeof(ModuleContentInitializerDelegate), module.Module, m));
                    Logger.Verbose("Found plain module initializer {Method}", m.Name);
                    continue;
                }

                throw new ArgumentException($"Expected a method with signature 'void XXX(IServiceResolver, IModuleInitializer), but found {m} in module {module.ModuleId}");
            }

            return actions;
        }

    }
}
