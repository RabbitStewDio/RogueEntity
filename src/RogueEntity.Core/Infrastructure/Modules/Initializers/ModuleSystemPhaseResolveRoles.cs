using System;
using System.Collections.Generic;
using System.Linq;
using RogueEntity.Core.Infrastructure.Modules.Helpers;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Modules.Initializers
{
    public class ModuleSystemPhaseResolveRoles<TGameContext> : IModuleEntityInitializationCallback<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem<TGameContext>>();
        readonly GlobalModuleEntityInformation<TGameContext> entityInfo;
        ModuleBase currentModule;
        ModuleInitializer<TGameContext> moduleInitializer;

        public ModuleSystemPhaseResolveRoles(in ModuleSystemPhaseInitModuleResult<TGameContext> p)
        {
            entityInfo = p.EntityInformation;
            moduleInitializer = p.ModuleInitializer;
        }

        public void Process(ReadOnlyListWrapper<ModuleRecord<TGameContext>> orderedModules)
        {
            CollectDeclaredRoles(orderedModules);
            ResolveEquivalenceRoles(orderedModules);
            PrintEntityRoles();
        }

        public void PrintEntityRoles()
        {
            foreach (var (entityType, _) in moduleInitializer.EntityInitializers)
            {
                if (entityInfo.TryGetModuleEntityInformation(entityType, out var entityInfoForType))
                {
                    Logger.Debug("[EntityStructure] Using Entity {EntityType}", entityType);
                    Logger.Debug("[EntityStructure]    Roles: {Count}", entityInfoForType.Roles.Count());
                    foreach (var role in entityInfoForType.Roles)
                    {
                        Logger.Debug("[EntityStructure]      - {Role}", role);
                    }
                    Logger.Debug("[EntityStructure]    Relations: {Count}", entityInfoForType.Relations.Count());
                    foreach (var relation in entityInfoForType.Relations)
                    {
                        Logger.Debug("[EntityStructure]      - {Relation}", relation);
                        if (entityInfoForType.TryQueryRelationTarget(relation, out var targetCollection))
                        {
                            foreach (var target in targetCollection)
                            {
                                Logger.Debug("[EntityStructure]          -> {RelationTarget}", target);
                            }
                        }
                    }
                }
            }
        }

        void CollectDeclaredRoles(ReadOnlyListWrapper<ModuleRecord<TGameContext>> open,
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
                foreach (var subject in moduleInitializer.EntityInitializers)
                {
                    currentModule = m.Module;
                    subject.callback(this);
                    currentModule = null;
                }

                diagnostics.Push(m.ModuleId);
                CollectDeclaredRoles(m.Dependencies, diagnostics);
                diagnostics.Pop();
            }
        }

        void IModuleEntityInitializationCallback<TGameContext>.PerformInitialization<TEntityId>(IModuleInitializationData<TGameContext, TEntityId> moduleContext)
        {
            var roleSet = entityInfo.CreateEntityInformation<TEntityId>();
            var subject = typeof(TEntityId);

            // record all explicitly declared role instances.
            if (currentModule.TryGetDeclaredRole<TEntityId>(out var roleRecord))
            {
                foreach (var role in roleRecord.Roles)
                {
                    roleSet.RecordRole(role, " as explicit declaration in module {Module}", currentModule.Id);
                }
            }

            // record all explicitly declared relationship instances.
            foreach (var relationRecord in currentModule.DeclaredEntityRelations)
            {
                foreach (var relTargetType in relationRecord)
                {
                    foreach (var relation in relationRecord[relTargetType])
                    {
                        roleSet.RecordRelation(relation, relTargetType);
                    }
                }
            }
            
            // attempt to resolve all template relation ship instances. 
            foreach (var relation in currentModule.RequiredRelations)
            {
                var subjectRole = relation.Subject;
                var targetRole = relation.Object;

                if (!roleSet.HasRole(subjectRole))
                {
                    continue;
                }

                if (relation.Id == ModuleRelationNames.ImpliedRoleRelationId)
                {
                    // those are special and handled in ResolveEquivalenceRoles in this class.
                    continue;
                }

                foreach (var relationTarget in entityInfo.FindEntityTypeForRole(targetRole))
                {
                    Logger.Debug("Entity {EntityType} requires relation {Relation} with target {Target} as explicit declaration in module {Module}",
                                 subject, relation.Id, relationTarget, currentModule.Id);
                    roleSet.RecordRelation(relation, relationTarget);
                }
            }
        }

        void ResolveEquivalenceRoles(ReadOnlyListWrapper<ModuleRecord<TGameContext>> open)
        {
            for (var index = open.Count - 1; index >= 0; index--)
            {
                var m = open[index];
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
                    var handled = entityInfo.RecordImpliedRelation(s, t, m.ModuleId);
                    if (!handled)
                    {
                        if (!r.Optional)
                        {
                            throw new Exception($"Unable to resolve equivalence relation {r} for module {m.ModuleId}. Subject role {s} is not active.");
                        }

                        Logger.Verbose("Role {Subject} is unused. Originally declared in module {Module} as alias to role {Target}, but no entity requires this role.", s.Id, m.ModuleId, t.Id);
                    }
                }
            }
        }
    }
}