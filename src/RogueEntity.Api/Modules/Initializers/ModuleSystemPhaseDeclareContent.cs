using System;
using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;
using RogueEntity.Api.Utils;
using Serilog;

namespace RogueEntity.Api.Modules.Initializers
{
    public class ModuleSystemPhaseDeclareContent : IModuleEntityInitializationCallback
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem>();
        readonly IReadOnlyDictionary<ModuleId, ModuleRecord> modulesById;
        readonly IServiceResolver serviceResolver;
        readonly ModuleInitializer moduleInitializer;
        readonly GlobalModuleEntityInformation entityInformation;

        readonly Dictionary<EntityRoleInstance, Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)>> roles;
        readonly Dictionary<EntityRelationInstance, Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)>> relations;

        public ModuleSystemPhaseDeclareContent(ModuleSystemPhaseInitModuleResult state,
                                               IServiceResolver serviceResolver,
                                               IReadOnlyDictionary<ModuleId, ModuleRecord> modulesById)
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

        void IModuleEntityInitializationCallback.PerformInitialization<TEntityId>(IModuleInitializationData<TEntityId> moduleContext)
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
                                           IEnumerable<ModuleRecord> declaringModules)
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

        void DeclareUniqueItems<TEntityId>(IModuleInitializationData<TEntityId> moduleContext)
            where TEntityId : IEntityKey
        {
            var ctx = serviceResolver.Resolve<IItemContextBackend<TEntityId>>();
            var items = new Dictionary<ItemDeclarationId, (ModuleId, IItemDeclaration)>();
            foreach (var (mod, b) in moduleContext.DeclaredBulkItems)
            {
                items[b.Id] = (mod, b);
            }

            foreach (var r in moduleContext.DeclaredReferenceItems)
            {
                items[r.itemDeclaration.Id] = r;
            }

            foreach (var (moduleId, itemDeclaration) in items.Values)
            {
                ctx.ItemRegistry.Register(itemDeclaration);
                RegisterTraitDependencies(roles, itemDeclaration.GetEntityRoles(), itemDeclaration.Id, moduleId);
                RegisterTraitDependencies(relations, itemDeclaration.GetEntityRelations(), itemDeclaration.Id, moduleId);
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

        IEnumerable<ModuleRecord> FindDeclaringModule(EntityRelation relation)
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

        class RegisterRelationTargetCallback : IModuleEntityInitializationCallback
        {
            readonly EntityRole role;
            readonly string messageTemplate;
            readonly object[] messageParameter;
            readonly GlobalModuleEntityInformation entityInformation;

            public RegisterRelationTargetCallback([NotNull] GlobalModuleEntityInformation entityInformation,
                                                  EntityRole role,
                                                  string messageTemplate,
                                                  params object[] messageParameter)
            {
                this.entityInformation = entityInformation ?? throw new ArgumentNullException(nameof(entityInformation));
                this.role = role;
                this.messageTemplate = messageTemplate;
                this.messageParameter = messageParameter;
            }

            public void PerformInitialization<TTarget>(IModuleInitializationData<TTarget> moduleContext)
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
