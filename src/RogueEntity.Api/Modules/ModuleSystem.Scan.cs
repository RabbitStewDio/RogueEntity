using System;
using System.Linq;
using System.Reflection;
using System.Text;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Utils;

namespace RogueEntity.Api.Modules
{
    public partial class ModuleSystem
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
        }

        public void ScanForModules(params string[] domain)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            foreach (var assembly in assemblies)
            {
                try
                {
                    ScanForModules(assembly, domain);
                }
                catch (ReflectionTypeLoadException e)
                {
                    Logger.Information(e, "Unable to load assembly {Assembly}", assembly.FullName);
                }
            }
        }
        
        public void ScanForModules(Assembly assembly, params string[] domain)
        {
            domain ??= new string[0];
            Logger.Verbose("Processing assembly {Assembly}", assembly.FullName);
            
            foreach (var typeInfo in assembly.DefinedTypes)
            {
                if (!typeof(ModuleBase).IsAssignableFrom(typeInfo))
                {
                    continue;
                }

                if (typeInfo.IsAbstract || typeInfo.IsGenericType)
                {
                    Logger.Verbose("Skipping abstract or generic module {Module}", typeInfo.FullName);
                    continue;
                }

                if (!(Activator.CreateInstance(typeInfo) is ModuleBase module))
                {
                    Logger.Verbose("Unable to auto-register module {Module} - Default Constructor is missing", typeInfo.FullName);
                    continue;
                }

                var attr = typeInfo.GetCustomAttribute<ModuleAttribute>();
                var moduleDomain = attr?.Domain;
                    
                if (DomainMatches(domain, moduleDomain))
                {
                    AddModule(module);
                }
                else
                {
                    Logger.Verbose("Skipping foreign domain module {Module}", typeInfo.FullName);
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

        static string PrintModuleDependencyList(ReadOnlyListWrapper<ModuleRecord> orderedRecords)
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