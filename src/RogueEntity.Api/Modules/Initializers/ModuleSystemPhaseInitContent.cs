using System;
using System.Collections.Generic;
using System.Reflection;
using EnTTSharp.Entities;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;
using RogueEntity.Api.Utils;
using Serilog;

namespace RogueEntity.Api.Modules.Initializers
{
    public class ModuleSystemPhaseInitContent<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem<TGameContext>>();

        readonly IServiceResolver serviceResolver;
        readonly GlobalModuleEntityInformation<TGameContext> moduleInfo;
        readonly ModuleInitializer<TGameContext> initializer;
        readonly ReadOnlyListWrapper<ModuleRecord<TGameContext>> orderedModules;

        public ModuleSystemPhaseInitContent(ModuleSystemPhaseInitModuleResult<TGameContext> previousResult,
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
            var activator = new ModuleEntityActivator(moduleInfo, initializer);
            foreach (var mod in orderedModules)
            {
                if (mod.InitializedContent)
                {
                    continue;
                }

                try
                {
                    initializer.CurrentModuleId = mod.ModuleId;

                    mod.InitializedContent = true;
                    var contentInitializers = CollectContentInitializers(mod.Module);
                    foreach (var mi in contentInitializers)
                    {
                        mi(in mip, initializer);
                    }
                    
                    foreach (var roleRecord in mod.Module.DeclaredEntityTypes)
                    {
                        roleRecord.Activate(activator);
                    }
                }
                finally
                {
                    initializer.CurrentModuleId = null;
                }
            }
        }

        class ModuleEntityActivator : IModuleEntityActivatorCallback
        {
            readonly GlobalModuleEntityInformation<TGameContext> moduleInfo;
            readonly ModuleInitializer<TGameContext> initializer;

            public ModuleEntityActivator(GlobalModuleEntityInformation<TGameContext> moduleInfo, ModuleInitializer<TGameContext> initializer)
            {
                this.moduleInfo = moduleInfo;
                this.initializer = initializer;
            }

            public void ActivateEntity<TEntity>(DeclaredEntityRoleRecord x) where TEntity : IEntityKey
            {
                initializer.DeclareEntityContext<TEntity>();
                var mi = moduleInfo.CreateEntityInformation<TEntity>();
                foreach (var role in x.Roles)
                {
                    mi.RecordRole(role);
                }
            }
        }

        static List<ModuleContentInitializerDelegate<TGameContext>> CollectContentInitializers(ModuleBase module)
        {
            var actions = new List<ModuleContentInitializerDelegate<TGameContext>>();
            foreach (var m in module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var attr = m.GetCustomAttribute<ContentInitializerAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (m.IsSameAction(typeof(ModuleInitializationParameter).MakeByRefType(), typeof(IModuleInitializer<TGameContext>)))
                {
                    actions.Add((ModuleContentInitializerDelegate<TGameContext>)Delegate.CreateDelegate(typeof(ModuleContentInitializerDelegate<TGameContext>), module, m));
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

                    throw new ArgumentException($"Expected a method with signature 'void XXX(IServiceResolver, IModuleInitializer<TGameContext>), but found {m} in module {module.Id}");
                }

                Logger.Verbose("Found generic content initializer {Method}", genericMethod);
                actions.Add((ModuleContentInitializerDelegate<TGameContext>)Delegate.CreateDelegate(typeof(ModuleContentInitializerDelegate<TGameContext>), module, genericMethod));
            }

            return actions;
        }

    }
}