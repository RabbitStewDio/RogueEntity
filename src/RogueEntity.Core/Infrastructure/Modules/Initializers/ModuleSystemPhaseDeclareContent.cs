using System;
using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules.Helpers;
using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Modules.Initializers
{
    public class ModuleSystemPhaseDeclareContent<TGameContext> : IModuleEntityInitializationCallback<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem<TGameContext>>();
        readonly IReadOnlyDictionary<ModuleId, ModuleRecord<TGameContext>> modulesById;
        readonly IServiceResolver serviceResolver;
        readonly ModuleInitializer<TGameContext> moduleInitializer;
        readonly GlobalModuleEntityInformation<TGameContext> entityInformation;

        readonly Dictionary<EntityRoleInstance, Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)>> roles;
        readonly Dictionary<EntityRelationInstance, Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)>> relations;

        public ModuleSystemPhaseDeclareContent(ModuleSystemPhaseInitModuleResult<TGameContext> state,
                                               IServiceResolver serviceResolver,
                                               IReadOnlyDictionary<ModuleId, ModuleRecord<TGameContext>> modulesById)
        {
            this.serviceResolver = serviceResolver;
            this.modulesById = modulesById;
            this.entityInformation = state.EntityInformation;
            this.moduleInitializer = state.ModuleInitializer;

            this.roles = new Dictionary<EntityRoleInstance, Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)>>();
            this.relations = new Dictionary<EntityRelationInstance, Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)>>();
        }

        public void DeclareItemTypes()
        {
            foreach (var (_, callback) in moduleInitializer.EntityInitializers)
            {
                callback(this);
            }
        }

        void IModuleEntityInitializationCallback<TGameContext>.PerformInitialization<TEntityId>(IModuleInitializationData<TGameContext, TEntityId> moduleContext)
        {
            DeclareUniqueItems(moduleContext);
            AddRolesFromTraits<TEntityId>();

            foreach (var rk in relations)
            {
                var rel = rk.Key;
                var evidence = CreateTraitEvidence(rk.Value);

                if (rel.SubjectEntityType == typeof(TEntityId))
                {
                    RecordRoleFromItem<TEntityId>(rel.Relation.Subject, " as subject in relation {Relation} as needed by {@Evidence}", rel, evidence);
                }

                if (!moduleInitializer.TryGetCallback(rel.ObjectEntityType, out var callback))
                {
                    throw new ArgumentException($"Relation target {rel.ObjectEntityType} is not a registered entity type");
                }

                if (rel.ObjectEntityType == typeof(TEntityId))
                {
                    callback(new RegisterRelationTargetCallback(entityInformation, rel.Relation.Object, " as target in relation {Relation} as needed by {@Evidence}", rel, evidence));
                }

                Logger.Debug("Registering {EntityType} relation {Relation} as needed by {Evidence}", typeof(TEntityId).Name, rel, evidence);
                RecordRelation<TEntityId>(rel.Relation, rel.ObjectEntityType);

                AddModuleDependencyFromTraits(rk.Value, FindDeclaringModule(rel.Relation));
            }
        }

        void AddRolesFromTraits<TEntityId>()
            where TEntityId : IEntityKey
        {
            foreach (var rk in roles)
            {
                var role = rk.Key;
                if (role.EntityType != typeof(TEntityId))
                {
                    continue;
                }
                
                RecordRoleFromItem<TEntityId>(role.Role, " as requested by {@Evidence}", CreateTraitEvidence(rk.Value));
                AddModuleDependencyFromTraits(rk.Value, FindDeclaringModule(role.Role));
            }
        }

        void AddModuleDependencyFromTraits(IReadOnlyDictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)> rk,
                                           IEnumerable<ModuleRecord<TGameContext>> declaringModules)
        {
            var declaringModulesList = declaringModules.ToList();
            foreach (var x in rk)
            {
                // module that declared the item.
                var moduleId = x.Value.Item1;
                if (!modulesById.TryGetValue(moduleId, out var moduleRecord))
                {
                    throw new ArgumentException($"Module {moduleId} is used but not known to the initializer");
                }

                foreach (var sourceModule in declaringModulesList)
                {
                    if (sourceModule.ModuleId != moduleId)
                    {
                        moduleRecord.AddDependency(sourceModule, "Item dependency of " + x.Key);
                    }
                }
            }
        }

        void RecordRelation<TEntityId>(EntityRelation relation, Type target)
        {
            var relationsAndTargets = this.entityInformation.CreateEntityInformation<TEntityId>();
            relationsAndTargets.RecordRelation(relation, target);
        }

        void RecordRoleFromItem<TEntityId>(EntityRole role, string messageTemplate, params object[] messageParameter)
        {
            var roleSet = this.entityInformation.CreateEntityInformation<TEntityId>();
            roleSet.RecordRole(role, messageTemplate, messageParameter);
        }

        void DeclareUniqueItems<TEntityId>(IModuleInitializationData<TGameContext, TEntityId> moduleContext)
            where TEntityId : IEntityKey
        {
            var ctx = serviceResolver.Resolve<IItemContextBackend<TGameContext, TEntityId>>();
            var items = new Dictionary<ItemDeclarationId, (ModuleId, IItemDeclaration)>();
            foreach (var (mod, b) in moduleContext.DeclaredBulkItems)
            {
                items[b.Id] = (mod, b);
            }

            foreach (var r in moduleContext.DeclaredReferenceItems)
            {
                items[r.Item2.Id] = r;
            }

            foreach (var (m, i) in items.Values)
            {
                ctx.ItemRegistry.Register(i);
                RegisterTraitDependencies(roles, i.GetEntityRoles(), i.Id, m);
                RegisterTraitDependencies(relations, i.GetEntityRelations(), i.Id, m);
            }
        }

        IEnumerable<ModuleRecord<TGameContext>> FindDeclaringModule(EntityRole role)
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

        IEnumerable<ModuleRecord<TGameContext>> FindDeclaringModule(EntityRelation relation)
        {
            // find module that declared this 
            foreach (var dep in modulesById.Values)
            {
                if (dep.Module.RequiredRelations.Contains(relation))
                {
                    yield return dep;
                }
            }
        }

        static void RegisterTraitDependencies<TKey>(Dictionary<TKey, Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)>> dataStore,
                                             IEnumerable<(ItemTraitId, TKey)> entries,
                                             ItemDeclarationId sourceRef,
                                             ModuleId moduleId)
        {
            foreach (var (traitId, role) in entries)
            {
                if (!dataStore.TryGetValue(role, out var data))
                {
                    data = new Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)>();
                    dataStore[role] = data;
                }

                if (!data.TryGetValue(sourceRef, out var evidence))
                {
                    evidence = (moduleId, new List<ItemTraitId>());
                    data[sourceRef] = evidence;
                }

                evidence.Item2.Add(traitId);
            }
        }

        class RegisterRelationTargetCallback : IModuleEntityInitializationCallback<TGameContext>
        {
            readonly EntityRole role;
            readonly string messageTemplate;
            readonly object[] messageParameter;
            readonly GlobalModuleEntityInformation<TGameContext> entityInformation;

            public RegisterRelationTargetCallback([NotNull] GlobalModuleEntityInformation<TGameContext> entityInformation,
                                                  EntityRole role,
                                                  string messageTemplate,
                                                  params object[] messageParameter)
            {
                this.entityInformation = entityInformation ?? throw new ArgumentNullException(nameof(entityInformation));
                this.role = role;
                this.messageTemplate = messageTemplate;
                this.messageParameter = messageParameter;
            }

            public void PerformInitialization<TTarget>(IModuleInitializationData<TGameContext, TTarget> moduleContext)
                where TTarget : IEntityKey
            {
                var roleSet = this.entityInformation.CreateEntityInformation<TTarget>();
                roleSet.RecordRole(role, messageTemplate, messageParameter);
            }
        }

        static List<(string, List<string>)> CreateTraitEvidence(Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)> backend)
        {
            var data = new List<(string, List<string>)>();
            foreach (var b in backend)
            {
                data.Add((b.Key.Id, b.Value.Item2.Select(e => e.Id).ToList()));
            }

            return data;
        }
    }
}