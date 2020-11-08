using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public partial class ModuleSystem<TGameContext>
    {
        /// <summary>
        ///    Records the existence of a module.
        /// </summary>
        /// <param name="module"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddModule(ModuleBase module)
        {
            if (initialized)
            {
                throw new InvalidOperationException("Cannot add modules to an already initialized system");
            }

            if (module.Id == null)
            {
                throw new ArgumentNullException("Module of type " + module.GetType() + " does not declare a module identifier.");
            }

            if (modulesById.TryGetValue(module.Id, out var existingModule))
            {
                if (existingModule.Module.GetType() != module.GetType())
                {
                    Logger.Warning("Module Id {ModuleId} is used by more than one module implementation. Registered Module: {RegisteredModule}, Conflicting Module: {ConflictingModule}",
                                   module.Id, existingModule.Module.GetType(), module.GetType());
                }
                return;
            }

            Logger.Debug("Registered module {ModuleId}", module.Id);

            var moduleRecord = new ModuleRecord(module);
            modulesById[module.Id] = moduleRecord;
            moduleRecord.ModuleInitializers.AddRange(CollectModuleInitializers(module));
            moduleRecord.ContentInitializers.AddRange(CollectContentInitializers(module));
        }

        public void ScanForModules()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            foreach (var assembly in assemblies)
            {
                ScanForModules(assembly);
            }
        }

        public void ScanForModules(Assembly assembly)
        {
            foreach (var typeInfo in assembly.DefinedTypes)
            {
                if (!typeof(ModuleBase).IsAssignableFrom(typeInfo))
                {
                    continue;
                }

                if (typeInfo.IsAbstract || typeInfo.IsGenericType)
                {
                    continue;
                }

                if (Activator.CreateInstance(typeInfo) is ModuleBase module)
                {
                    AddModule(module);
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

                if (m.IsSameAction(typeof(IServiceResolver), typeof(IModuleInitializer<TGameContext>)))
                {
                    actions.Add((ModuleInitializerDelegate<TGameContext>)Delegate.CreateDelegate(typeof(ModuleInitializerDelegate<TGameContext>), module, m));
                    Logger.Verbose("Found plain module initializer {Method}", m);
                    continue;
                }

                if (!m.IsSameGenericAction(new[] {typeof(TGameContext)},
                                           out var genericMethod, out var errorHint,
                                           typeof(IServiceResolver), typeof(IModuleInitializer<TGameContext>)))
                {
                    if (!string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(errorHint);
                    }

                    throw new ArgumentException($"Expected a method with signature 'void XXX(IServiceResolver, IModuleInitializer<TGameContext>), but found {m} in module {module.Id}");
                }

                Logger.Verbose("Found generic module initializer {Method}", genericMethod);
                actions.Add((ModuleInitializerDelegate<TGameContext>) Delegate.CreateDelegate(typeof(ModuleInitializerDelegate<TGameContext>), module, genericMethod));
            }

            return actions;
        }

        List<ModuleContentInitializerDelegate<TGameContext>> CollectContentInitializers(ModuleBase module)
        {
            var actions = new List<ModuleContentInitializerDelegate<TGameContext>>();
            foreach (var m in module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var attr = m.GetCustomAttribute<ContentInitializerAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (m.IsSameAction(typeof(IServiceResolver), typeof(IModuleInitializer<TGameContext>)))
                {
                    actions.Add((ModuleContentInitializerDelegate<TGameContext>)Delegate.CreateDelegate(typeof(ModuleContentInitializerDelegate<TGameContext>), module, m));
                    Logger.Verbose("Found plain module initializer {Method}", m);
                    continue;
                }

                if (!m.IsSameGenericAction(new[] {typeof(TGameContext)},
                                           out var genericMethod, out var errorHint,
                                           typeof(IServiceResolver), typeof(IModuleInitializer<TGameContext>)))
                {
                    if (!string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(errorHint);
                    }

                    throw new ArgumentException($"Expected a method with signature 'void XXX(IServiceResolver, IModuleInitializer<TGameContext>), but found {m} in module {module.Id}");
                }

                Logger.Verbose("Found generic content initializer {Method}", genericMethod);
                actions.Add((ModuleContentInitializerDelegate<TGameContext>) Delegate.CreateDelegate(typeof(ModuleContentInitializerDelegate<TGameContext>), module, genericMethod));
            }

            return actions;
        }
        
    }
}