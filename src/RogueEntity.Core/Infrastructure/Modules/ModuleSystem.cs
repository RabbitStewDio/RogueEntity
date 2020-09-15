using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public class ModuleSystem<TGameContext>
    {
        readonly List<ModuleRecord> modules;
        readonly Dictionary<string, ModuleRecord> modulesById;
        bool initialized;

        public ModuleSystem()
        {
            modulesById = new Dictionary<string, ModuleRecord>();
            modules = new List<ModuleRecord>();
        }

        public void AddModule(IModule<TGameContext> module)
        {
            if (initialized)
            {
                throw new InvalidOperationException("Cannot add modules to an already initialized system");
            }

            if (modulesById.ContainsKey(module.Id))
            {
                return;
            }

            var moduleRecord = new ModuleRecord(module);
            modulesById[module.Id] = moduleRecord;
            modules.Add(moduleRecord);
        }

        public void Initialize(TGameContext context, IModuleInitializer<TGameContext> initializer)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            if (modules.Count == 0)
            {
                // No modules declared. Its OK if you dont want to use the module system.
                return;
            }

            CollectModuleDependencies();

            List<ModuleRecord> openModules = new List<ModuleRecord>();
            openModules.AddRange(modules.Where(m => !m.HasDependentModules));
            if (openModules.Count == 0)
            {
                // We expect a directed acyclical graph. This means someone messed up.
                throw new ArgumentException("Unable to identify top level modules. All modules appear to be dependencies of other modules.");
            }

            var orderedModules = ComputeModuleOrder(openModules);
            Console.WriteLine(string.Join(", ", orderedModules.Select(m => m.ModuleId)));

            PropagateEntities(orderedModules);

            foreach(var mod in orderedModules)
            {
                if (mod.InitializedEntities)
                {
                    continue;
                }

                mod.InitializedEntities = true;

                foreach (var e in mod.Entities)
                {
                    mod.Module.Initialize(e, context, initializer);
                }
            }

            foreach (var mod in orderedModules)
            {
                if (mod.InitializedContent)
                {
                    continue;
                }

                mod.InitializedContent = true;

                mod.Module.InitializeContent(context, initializer);
            }
        }

        /// <summary>
        ///   Returns the modules as a flat list in depth first traversal order.
        ///   Higher level modules are listed before their lower level dependencies.
        ///   There is no guarantee of order between branches.
        /// </summary>
        /// <param name="open"></param>
        /// <param name="diagnostics"></param>
        /// <returns></returns>
        List<ModuleRecord> ComputeModuleOrder(List<ModuleRecord> open, Stack<string> diagnostics = null)
        {
            if (diagnostics == null)
            {
                diagnostics = new Stack<string>();
            }

            var result = new List<ModuleRecord>();
            foreach (var o in open)
            {
                if (diagnostics.Contains(o.ModuleId))
                {
                    throw new Exception($"Unable to resolve valid dependency chain for module {o.ModuleId} while processing dependency chain [{string.Join("], [", diagnostics)}]");
                }

                if (o.Resolved)
                {
                    continue;
                }

                o.Resolved = true;

                diagnostics.Push(o.ModuleId);
                var moduleOrder = ComputeModuleOrder(o.Dependencies, diagnostics);
                foreach (var mo in moduleOrder)
                {
                    if (!result.Contains(mo))
                    {
                        result.AddRange(moduleOrder);
                    }
                }

                result.Add(o);
                diagnostics.Pop();
            }

            return result;
        }


        void PropagateEntities(List<ModuleRecord> orderedModules)
        {
            for (var index = orderedModules.Count - 1; index >= 0; index--)
            {
                var m = orderedModules[index];
                Console.WriteLine("Process" + m.ModuleId);

                foreach (var md in m.Dependencies)
                {
                    foreach (var e in m.Entities)
                    {
                        Console.WriteLine("Propagate " + e + " to " + md.ModuleId);
                        md.Entities.Add(e);
                    }
                }

                foreach (var md in m.Module.ModuleDependencies)
                {
                    if (!modulesById.TryGetValue(md.ModuleId, out var mdr))
                    {
                        throw new InvalidOperationException($"Missing module {md.ModuleId}");
                    }

                    if (md.EntityType.TryGetValue(out var entityType))
                    {
                        Console.WriteLine("Declare " + entityType + " to " + mdr.ModuleId);
                        mdr.Entities.Add(entityType);
                    }
                }
            }
        }

        void CollectModuleDependencies()
        {
            foreach (var m in modules)
            {
                foreach (var md in m.Module.ModuleDependencies)
                {
                    if (!modulesById.TryGetValue(md.ModuleId, out var mr))
                    {
                        throw new ModuleInitializationException($"Module '{m.ModuleId}' declared a missing dependency to module '{md.ModuleId}'");
                    }

                    m.Dependencies.Add(mr);
                    mr.HasDependentModules = true;
                }
            }
        }

        class ModuleRecord
        {
            public readonly string ModuleId;
            public readonly IModule<TGameContext> Module;
            public readonly List<ModuleRecord> Dependencies;
            public bool HasDependentModules { get; set; }
            public HashSet<Type> Entities { get; }

            public bool Resolved { get; set; }
            public bool InitializedEntities { get; set; }
            public bool InitializedContent { get; set; }

            public ModuleRecord(IModule<TGameContext> module)
            {
                Module = module;
                ModuleId = module.Id;
                Dependencies = new List<ModuleRecord>();
                Entities = new HashSet<Type>();
            }
        }
    }
}