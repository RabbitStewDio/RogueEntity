using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules.Initializers;

namespace RogueEntity.Core.Infrastructure.Modules
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public partial class ModuleSystem<TGameContext>
    {
        string PrintModuleDependencyList(List<ModuleRecord> orderedRecords)
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

        void AddModuleDependencyFromRoleUsage(ModuleRecord requestingModule, EntityRole role)
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

        IEnumerable<ModuleRecord> FindDeclaringModule(EntityRole role)
        {
            // find module that declared this 
            foreach (var dep in modulesById.Values)
            {
                if (dep.Module.RequiredRoles.Contains(role))
                {
                    yield return dep;
                }
            }
        }

        List<ModuleRecord> CollectRootLevelModules()
        {
            var openModules = new List<ModuleRecord>();
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
                throw new ArgumentException(
                    "Unable to identify top level modules. All modules appear to be dependencies of some other modules.");
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
        List<ModuleRecord> ComputeModuleOrder(List<ModuleRecord> open, Stack<ModuleId> diagnostics = null)
        {
            if (diagnostics == null)
            {
                diagnostics = new Stack<ModuleId>();
            }

            var result = new List<ModuleRecord>();
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

        void RecordRelation(Type subject, EntityRelation relation, Type target)
        {
            if (!relationsPerType.TryGetValue(subject, out var relationsAndTargets))
            {
                relationsAndTargets = new EntityRelationRecord(subject);
                relationsPerType[subject] = relationsAndTargets;
            }

            relationsAndTargets.RecordRelation(relation, target);
        }

        void CollectDeclaredRoles(List<ModuleRecord> open,
                                  Stack<ModuleId> diagnostics = null)
        {
            if (diagnostics == null)
            {
                diagnostics = new Stack<ModuleId>();
            }

            foreach (var m in open)
            {
                if (diagnostics.Contains(m.ModuleId))
                {
                    throw new Exception($"Infinite loop in dependency chain for module {m.ModuleId} while processing dependency chain [{string.Join("], [", diagnostics)}]");
                }

                if (m.ResolvedRoles)
                {
                    continue;
                }

                m.ResolvedRoles = true;

                // Collect all active entities for this module.
                foreach (var subject in m.Module.DeclaredEntityTypes)
                {
                    if (!rolesPerType.TryGetValue(subject, out var roleSet))
                    {
                        roleSet = new EntityRoleRecord(subject);
                        rolesPerType[subject] = roleSet;
                        entityTypeMetaData[subject] = m.Module;
                    }

                    if (m.Module.TryGetEntityRecord(subject, out var roleRecord))
                    {
                        foreach (var role in roleRecord.Roles)
                        {
                            roleSet.RecordRole(role, " as explicit declaration in module {Module}", m.ModuleId);
                        }
                    }

                    foreach (var relation in m.Module.RequiredRelations)
                    {
                        var subjectRole = relation.Subject;
                        var targetRole = relation.Object;

                        if (!roleSet.HasRole(subjectRole))
                        {
                            continue;
                        }

                        if (relation.Id == ModuleRelationNames.ImpliedRoleRelationId)
                        {
                            // those are special and handled elsewhere.
                            continue;
                        }

                        foreach (var relationTarget in FindEntityTypeForRole(targetRole))
                        {
                            Logger.Debug("Entity {EntityType} requires relation {Relation} with target {Target} as explicit declaration in module {Module}",
                                         subject, relation.Id, relationTarget, m.ModuleId);
                            RecordRelation(subject, relation, relationTarget);
                        }
                    }
                }

                diagnostics.Push(m.ModuleId);
                CollectDeclaredRoles(m.Dependencies, diagnostics);
                diagnostics.Pop();
            }
        }

        List<Type> FindEntityTypeForRole(EntityRole role)
        {
            List<Type> types = new List<Type>();
            foreach (var e in rolesPerType)
            {
                if (e.Value.HasRole(role))
                {
                    types.Add(e.Key);
                }
            }

            return types;
        }

        void ResolveEquivalenceRoles(List<ModuleRecord> open,
                                     Stack<ModuleId> diagnostics = null)
        {
            if (diagnostics == null)
            {
                diagnostics = new Stack<ModuleId>();
            }

            for (var index = open.Count - 1; index >= 0; index--)
            {
                var m = open[index];
                if (diagnostics.Contains(m.ModuleId))
                {
                    throw new Exception($"Infinite loop in dependency chain for module {m.ModuleId} while processing dependency chain [{string.Join("], [", diagnostics)}]");
                }

                if (m.ResolvedEquivalence)
                {
                    continue;
                }

                m.ResolvedEquivalence = true;

                foreach (var r in m.Module.RequiredRelations)
                {
                    if (r.Id != ModuleRelationNames.ImpliedRoleRelationId)
                    {
                        continue;
                    }

                    var s = r.Subject;
                    var t = r.Object;
                    var handled = false;
                    foreach (var knownRoles in rolesPerType.Values)
                    {
                        if (knownRoles.RecordImpliedRelation(s, t, m.ModuleId))
                        {
                            handled = true;
                        }
                    }

                    if (!handled)
                    {
                        if (!r.Optional)
                        {
                            throw new Exception($"Unable to resolve equivalence relation {r} for module {m.ModuleId} while resolving equivalence roles [{string.Join("], [", diagnostics)}]");
                        }

                        Logger.Verbose("Role {Subject} is unused. Originally declared in module {Module} as alias to role {Target}, but no entity requires this role.", s.Id, m.ModuleId, t.Id);
                    }
                }
            }
        }
    }
}