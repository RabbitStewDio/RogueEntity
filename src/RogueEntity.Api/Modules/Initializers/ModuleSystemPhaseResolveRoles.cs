using System;
using System.Collections.Generic;
using System.Linq;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Utils;
using Serilog;

namespace RogueEntity.Api.Modules.Initializers
{
    public class ModuleSystemPhaseResolveRoles : IModuleEntityInitializationCallback
    {
        static readonly ILogger logger = SLog.ForContext<ModuleSystem>();
        readonly GlobalModuleEntityInformation entityInfo;
        IModule? currentModule;
        readonly ModuleInitializer moduleInitializer;

        public ModuleSystemPhaseResolveRoles(in ModuleSystemPhaseInitModuleResult p)
        {
            entityInfo = p.EntityInformation;
            moduleInitializer = p.ModuleInitializer;
        }

        public void Process(ReadOnlyListWrapper<ModuleRecord> orderedModules)
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
                    logger.Debug("[EntityStructure] Using Entity {EntityType}", entityType);
                    logger.Debug("[EntityStructure]    Roles: {Count}", entityInfoForType.Roles.Count());
                    foreach (var role in entityInfoForType.Roles)
                    {
                        logger.Debug("[EntityStructure]      - {Role}", role);
                    }
                    logger.Debug("[EntityStructure]    Relations: {Count}", entityInfoForType.Relations.Count());
                    foreach (var relation in entityInfoForType.Relations)
                    {
                        logger.Debug("[EntityStructure]      - {Relation}", relation);
                        if (entityInfoForType.TryQueryRelationTarget(relation, out var targetCollection))
                        {
                            foreach (var target in targetCollection)
                            {
                                logger.Debug("[EntityStructure]          -> {RelationTarget}", target);
                            }
                        }
                    }
                }
            }
        }

        void CollectDeclaredRoles(ReadOnlyListWrapper<ModuleRecord> open,
                                  Stack<ModuleId>? diagnostics = null)
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

        void IModuleEntityInitializationCallback.PerformInitialization<TEntityId>(IModuleInitializationData<TEntityId> moduleContext)
        {
            if (currentModule == null) return;
            
            var roleSet = entityInfo.CreateEntityInformation<TEntityId>();
            var subject = typeof(TEntityId);

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
                    logger.Debug("Entity {EntityType} requires relation {Relation} with target {Target} as explicit declaration in module {Module}",
                                 subject, relation.Id, relationTarget, currentModule.Id);
                    roleSet.RecordRelation(relation, relationTarget);
                }
            }
        }

        void ResolveEquivalenceRoles(ReadOnlyListWrapper<ModuleRecord> open)
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
                        logger.Verbose("Role {Subject} is unused. Originally declared in module {Module} as alias to role {Target}, but no entity requires this role", s.Id, m.ModuleId, t.Id);
                    }
                }
            }
        }
    }
}