using System;
using System.Linq;
using System.Reflection;
using System.Text;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Helpers;
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

            var moduleRecord = new ModuleRecord<TGameContext>(module);
            modulesById[module.Id] = moduleRecord;
        }

        public void ScanForModules(params string[] domain)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            foreach (var assembly in assemblies)
            {
                ScanForModules(assembly, domain);
            }
        }

        public void ScanForModules(Assembly assembly, params string[] domain)
        {
            domain ??= new string[0];
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

                if (!(Activator.CreateInstance(typeInfo) is ModuleBase module))
                {
                    continue;
                }

                var attr = typeInfo.GetCustomAttribute<ModuleAttribute>();
                var moduleDomain = attr?.Domain;
                    
                if (DomainMatches(domain, moduleDomain))
                {
                    AddModule(module);
                }
            }
        }

        static bool DomainMatches(string[] domainSpec, string moduleDomain)
        {
            if (string.IsNullOrEmpty(moduleDomain))
            {
                // considered to be a framework module.
                return true;
            }

            if (domainSpec.Length == 0)
            {
                // we are only looking for framework modules at this point.
                return false;
            }

            // check whether you are one of the droids I am looking for.
            return domainSpec.Contains(moduleDomain);
        }

        static string PrintModuleDependencyList(ReadOnlyListWrapper<ModuleRecord<TGameContext>> orderedRecords)
        {
            StringBuilder b = new StringBuilder();
            foreach (var r in orderedRecords)
            {
                b.Append(r.ModuleId);
                b.Append(" [");
                b.Append(r.DependencyDepth);
                b.Append(": ");
                b.Append(string.Join(", ", r.Dependencies.Select(xx => xx.ModuleId)));
                b.Append(" ]");
                b.AppendLine();
            }

            return b.ToString();
        }

    }
}