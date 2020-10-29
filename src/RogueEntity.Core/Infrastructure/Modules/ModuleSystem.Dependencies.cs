using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

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

        List<ModuleRecord> CollectRootLevelModules()
        {
            List<ModuleRecord> openModules = new List<ModuleRecord>();
            foreach (var m in modulePool)
            {
                if (m.HasDependentModules)
                    continue;

                openModules.Add(m);
            }

            if (openModules.Count == 0)
            {
                // We expect a directed acyclical graph. This means someone messed up.
                throw new ArgumentException(
                    "Unable to identify top level modules. All modules appear to be dependencies of some other modules.");
            }

            return openModules;
        }

        void RecordRelation(Type subject, EntityRelation relation, Type target)
        {
            if (!relationsPerType.TryGetValue(subject, out var relationsAndTargets))
            {
                relationsAndTargets = new Dictionary<EntityRelation, HashSet<Type>>();
                relationsPerType[subject] = relationsAndTargets;
            }

            if (!relationsAndTargets.TryGetValue(relation, out var targets))
            {
                targets = new HashSet<Type>();
                relationsAndTargets[relation] = targets;
            }

            targets.Add(target);
        }

        void CollectDeclaredRoles(List<ModuleRecord> open,
                                  Stack<string> diagnostics = null)
        {
            if (diagnostics == null)
            {
                diagnostics = new Stack<string>();
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
                    if (m.Module.TryGetEntityRecord(subject, out var roleRecord))
                    {
                        if (!rolesPerType.TryGetValue(subject, out var roleSet))
                        {
                            roleSet = new Dictionary<EntityRole, ModuleRecord>();
                            rolesPerType[subject] = roleSet;
                        }

                        foreach (var role in roleRecord.Roles)
                        {
                            roleSet[role] = m;
                        }
                    }

                    if (m.Module.TryGetRelationRecord(subject, out var relationRecord))
                    {
                        foreach (var relationTarget in relationRecord)
                        {
                            if (!relationRecord.TryGet(relationTarget, out var relationsInRecord))
                            {
                                continue;
                            }

                            foreach (var r in relationsInRecord)
                            {
                                RecordRelation(subject, r, relationTarget);
                            }
                        }
                    }
                }


                diagnostics.Push(m.ModuleId);
                CollectDeclaredRoles(m.Dependencies, diagnostics);
                diagnostics.Pop();
            }
        }

        void ResolveEquivalenceRoles(List<ModuleRecord> open,
                                     Stack<string> diagnostics = null)
        {
            if (diagnostics == null)
            {
                diagnostics = new Stack<string>();
            }

            foreach (var m in open)
            {
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
                        if (knownRoles.TryGetValue(s, out var module))
                        {
                            if (!knownRoles.ContainsKey(t))
                            {
                                knownRoles.Add(t, m);
                            }

                            handled = true;
                        }
                    }

                    if (!handled)
                    {
                        if (!r.Optional)
                        {
                            throw new Exception($"Unable to resolve equivalence relation {r} for module {m.ModuleId} while resolving equivalence roles [{string.Join("], [", diagnostics)}]");
                        }
                        else
                        {
                            Logger.Debug("Role {Subject} is not used.", s);
                        }
                    }
                }
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