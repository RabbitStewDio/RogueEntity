using System;
using System.Collections.Generic;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules.Helpers;

namespace RogueEntity.Core.Infrastructure.Modules.Initializers
{
    public class ModuleSystemPhaseInit<TGameContext>
    {
        readonly IReadOnlyDictionary<ModuleId, ModuleRecord<TGameContext>> modulesById;

        public ModuleSystemPhaseInit(IReadOnlyDictionary<ModuleId, ModuleRecord<TGameContext>> modulesById)
        {
            this.modulesById = modulesById;
        }

        public List<ModuleRecord<TGameContext>> CreateModulesSortedByInitOrder()
        {
            PopulateModuleDependencies();
            var dependencyRoots = CollectRootLevelModules(modulesById.Values);
            return ComputeModuleOrder(dependencyRoots);
        }
        
        void PopulateModuleDependencies()
        {
            foreach (var m in modulesById.Values)
            {
                m.ResolvedOrder = false;

                foreach (var md in m.Module.ModuleDependencies)
                {
                    if (!modulesById.TryGetValue(md.ModuleId, out var mr))
                    {
                        throw new ModuleInitializationException($"Module '{m.ModuleId}' declared a missing dependency to module '{md.ModuleId}'");
                    }

                    m.AddDependency(mr, "Explicit Dependency");
                    mr.IsUsedAsDependency = true;
                }
            }

            foreach (var m in modulesById.Values)
            {
                foreach (var r in m.Module.RequiredRelations)
                {
                    var subject = r.Subject;
                    var obj = r.Object;

                    if (!m.Module.RequiredRoles.Contains(subject))
                    {
                        // find module that declared this 
                        foreach (var dep in modulesById.Values)
                        {
                            if (dep == m)
                            {
                                continue;
                            }

                            if (dep.Module.RequiredRoles.Contains(subject))
                            {
                                m.AddDependency(dep, "Implied Relation Subject Dependency");
                            }
                        }
                    }

                    AddModuleDependencyFromRoleUsage(m, obj);
                }
            }
        }

        void AddModuleDependencyFromRoleUsage(ModuleRecord<TGameContext> requestingModule, EntityRole role)
        {
            if (!requestingModule.Module.RequiredRoles.Contains(role))
            {
                // find module that declared this 
                foreach (var dep in modulesById.Values)
                {
                    if (dep == requestingModule)
                    {
                        continue;
                    }

                    if (dep.Module.RequiredRoles.Contains(role))
                    {
                        requestingModule.AddDependency(dep, "Implied Role Dependency");
                    }
                }
            }
        }

        
        IEnumerable<ModuleRecord<TGameContext>> CollectRootLevelModules(IEnumerable<ModuleRecord<TGameContext>> contentModulePool)
        {
            var openModules = new List<ModuleRecord<TGameContext>>();
            foreach (var m in contentModulePool)
            {
                if (m.IsUsedAsDependency)
                {
                    continue;
                }

                openModules.Add(m);
            }

            if (openModules.Count == 0)
            {
                // We expect a directed acyclic graph. This means someone messed up.
                throw new ArgumentException("Unable to identify top level modules. All modules appear to be dependencies of some other modules.");
            }

            return openModules;
        }

        /// <summary>
        ///   Returns the modules as a flat list in depth first traversal order.
        ///   Higher level modules are listed before their lower level dependencies.
        ///   There is no guarantee of order between branches.
        /// </summary>
        /// <param name="open"></param>
        /// <param name="diagnostics"></param>
        /// <returns></returns>
        List<ModuleRecord<TGameContext>> ComputeModuleOrder(IEnumerable<ModuleRecord<TGameContext>> open, Stack<ModuleId> diagnostics = null)
        {
            if (diagnostics == null)
            {
                diagnostics = new Stack<ModuleId>();
            }

            var result = new List<ModuleRecord<TGameContext>>();
            foreach (var o in open)
            {
                if (diagnostics.Contains(o.ModuleId))
                {
                    throw new Exception($"Infinite loop in dependency chain for module {o.ModuleId} while processing dependency chain [{string.Join("], [", diagnostics)}]");
                }

                if (o.ResolvedOrder)
                {
                    continue;
                }

                o.ResolvedOrder = true;

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

    }
}