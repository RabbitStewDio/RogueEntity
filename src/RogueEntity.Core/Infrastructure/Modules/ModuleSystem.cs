using System;
using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules.Initializers;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Utils;
using Serilog;
using Serilog.Events;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public partial class ModuleSystem<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem<TGameContext>>();

        readonly IServiceResolver serviceResolver;
        readonly List<ModuleRecord> contentModulePool;
        readonly Dictionary<ModuleId, ModuleRecord> modulesById;
        readonly Dictionary<Type, EntityRoleRecord> rolesPerType;
        readonly Dictionary<Type, ModuleBase> entityTypeMetaData;
        readonly Dictionary<Type, EntityRelationRecord> relationsPerType;
        readonly ModuleSystemSystemRegistrations<TGameContext> registrations;
        bool initialized;

        public ModuleSystem([NotNull] IServiceResolver serviceResolver)
        {
            modulesById = new Dictionary<ModuleId, ModuleRecord>();
            contentModulePool = new List<ModuleRecord>();

            this.serviceResolver = serviceResolver ?? throw new ArgumentNullException(nameof(serviceResolver));

            entityTypeMetaData = new Dictionary<Type, ModuleBase>();
            rolesPerType = new Dictionary<Type, EntityRoleRecord>();
            relationsPerType = new Dictionary<Type, EntityRelationRecord>();
            registrations = new ModuleSystemSystemRegistrations<TGameContext>();
        }

        public IGameLoopSystemInformation<TGameContext> Initialize(TGameContext context)
        {
            var moduleInitializer = InitializeModules();
            var globalSystems = new Dictionary<EntitySystemId, IGlobalSystemDeclaration<TGameContext>>();
            foreach (var globalSystem in moduleInitializer.GlobalSystems)
            {
                if (globalSystems.TryGetValue(globalSystem.Id, out var entry))
                {
                    if (entry.InsertionOrder >= globalSystem.InsertionOrder)
                    {
                        continue;
                    }
                }

                globalSystems[globalSystem.Id] = globalSystem;
            }

            var mip = new ModuleInitializationParameter(new GlobalModuleEntityInformation(rolesPerType, relationsPerType), serviceResolver);
            foreach (var globalSystem in globalSystems.Values.OrderBy(e => e.InsertionOrder))
            {
                try
                {
                    registrations.EnterContext(globalSystem);
                    globalSystem.SystemRegistration(mip, registrations);
                }
                finally
                {
                    registrations.LeaveContext();
                }
            }

            foreach (var entity in moduleInitializer.EntityInitializers)
            {
                if (!rolesPerType.TryGetValue(entity.entityType, out var roles))
                {
                    Logger.Warning("Skipping initialization of entity systems for {EntityType} as it has no roles declared", entity.entityType);
                    continue;
                }
                
                if (!relationsPerType.TryGetValue(entity.entityType, out var relations))
                {
                    relations = new EntityRelationRecord(entity.entityType);
                    relationsPerType[entity.entityType] = relations;
                }

                var emip = new ModuleInitializationParameter(new ModuleEntityInformation(entity.entityType, roles, relations), serviceResolver);
                var handler = new EntitySystemRegistrationHandler(emip, registrations);
                entity.callback(handler);
            }

            return registrations;
        }

        class EntitySystemRegistrationHandler : IModuleEntityInitializationCallback<TGameContext>
        {
            readonly ModuleInitializationParameter mip;
            readonly ModuleSystemSystemRegistrations<TGameContext> registrations;

            public EntitySystemRegistrationHandler(ModuleInitializationParameter mip,
                                                   ModuleSystemSystemRegistrations<TGameContext> registrations)
            {
                this.mip = mip;
                this.registrations = registrations;
            }

            public void PerformInitialization<TEntityId>(IModuleInitializationData<TGameContext, TEntityId> moduleContext)
                where TEntityId : IEntityKey
            {
                var ctx = mip.ServiceResolver.Resolve<IItemContextBackend<TGameContext, TEntityId>>();

                var entitySystems = new Dictionary<EntitySystemId, IEntitySystemDeclaration<TGameContext, TEntityId>>();


                foreach (var system in moduleContext.EntitySystems)
                {
                    if (entitySystems.TryGetValue(system.Id, out var entry))
                    {
                        if (entry.InsertionOrder >= system.InsertionOrder)
                        {
                            continue;
                        }
                    }

                    entitySystems[system.Id] = system;
                }

                var sortedEntries = entitySystems.Values.OrderBy(e => e.InsertionOrder).ToList();
                foreach (var system in sortedEntries)
                {
                    try
                    {
                        registrations.EnterContext(system);
                        system.EntityRegistration?.Invoke(mip, ctx.EntityRegistry);
                    }
                    finally
                    {
                        registrations.LeaveContext();
                    }
                }

                foreach (var system in sortedEntries)
                {
                    try
                    {
                        registrations.EnterContext(system);
                        system.EntitySystemRegistration?.Invoke(mip, registrations, ctx.EntityRegistry);
                    }
                    finally
                    {
                        registrations.LeaveContext();
                    }
                }
            }
        }

        public IModuleInitializationData<TGameContext> InitializeModules()
        {
            if (initialized)
            {
                throw new InvalidOperationException("Initialization can be called only once");
            }

            initialized = true;

            // Collect all content modules. Skips pure framework modules, as 
            // those should be pulled in as dependency later if their provided features are needed.
            foreach (var m in modulesById.Values)
            {
                if (m.Module.IsFrameworkModule)
                {
                    continue;
                }

                contentModulePool.Add(m);
            }

            var moduleInitializer = new ModuleInitializer<TGameContext>();
            if (contentModulePool.Count == 0)
            {
                // No content modules declared. Its OK, you apparently dont want to use the module system.
                return moduleInitializer;
            }

            PopulateModuleDependencies();

            var openModules = CollectRootLevelModules();
            var orderedModules = ComputeModuleOrder(openModules);
            Logger.Debug("Processing Modules in order: \n{Modules}", PrintModuleDependencyList(orderedModules));

            // 1. Setup global functions. Those provide global services or configurations etc, and only depend on the context object. 
            InitializeModule(moduleInitializer, orderedModules);

            // 2. Initialize content. This sets up EntityDeclarations and thus tells the system which roles are used by each entity key
            //    encountered by the code.
            InitializeModuleContent(moduleInitializer, orderedModules);

            DeclareItemTypes(moduleInitializer);
            
            // 4. Based on the module information gathered, we can now resolve all roles and relations. First, some sorting. 
            CollectDeclaredRoles(orderedModules);
            ResolveEquivalenceRoles(orderedModules);

            // Recompute module dependencies.
            PopulateModuleDependencies();
            
            orderedModules = ComputeModuleOrder(openModules);
            Logger.Debug("Processing Modules in order: \n{Modules}", PrintModuleDependencyList(orderedModules));

            // 5. Now start initializing trait systems for each entity role.
            InitializeModuleRoles(moduleInitializer, orderedModules);
            InitializeModuleRelations(moduleInitializer, orderedModules);

            return moduleInitializer;
        }
        
        void DeclareItemTypes(ModuleInitializer<TGameContext> moduleInitializer)
        {
            foreach (var (entityType, callback) in moduleInitializer.EntityInitializers)
            {
                var handler = new ItemTypeRegistrationHandler(serviceResolver);
                callback(handler);

                foreach (var rk in handler.Roles)
                {
                    var role = rk.Key;
                    var roleEntity = role.EntityType;
                    RecordRoleFromItem(roleEntity, role.Role, " as requested by {@Evidence}", CreateTraitEvidence(rk.Value));

                    foreach (var x in rk.Value)
                    {
                        // module that declared the item.
                        var moduleId = x.Value.Item1;
                        if (modulesById.TryGetValue(moduleId, out var moduleRecord))
                        {
                            foreach (var sourceModule in FindDeclaringModule(role.Role))
                            {
                                if (sourceModule.ModuleId != moduleId)
                                {
                                    moduleRecord.AddDependency(sourceModule, "Item dependency of " + x.Key);
                                }                            
                            }
                        }
                    }
                }

                foreach (var rk in handler.Relations)
                {
                    var rel = rk.Key;
                    var evidence = CreateTraitEvidence(rk.Value);
                    RecordRoleFromItem(rel.ObjectEntityType, rel.Relation.Object, " as target in relation {Relation} as needed by {@Evidence}", rel, evidence);;
                    RecordRoleFromItem(rel.SubjectEntityType, rel.Relation.Subject, " as subject in relation {Relation} as needed by {@Evidence}", rel, evidence);

                    Logger.Debug("Registering {EntityType} relation {Relation} as needed by {Evidence}", entityType.Name, rel, evidence);
                    RecordRelation(rel.SubjectEntityType, rel.Relation, rel.ObjectEntityType);
                }
            }
        }

        void RecordRoleFromItem(Type roleEntity, EntityRole role, string messageTemplate, params object[] messageParameter)
        {
            if (!rolesPerType.TryGetValue(roleEntity, out var roleSet))
            {
                roleSet = new EntityRoleRecord(roleEntity);
                rolesPerType[roleEntity] = roleSet;
            }

            roleSet.RecordRole(role, messageTemplate, messageParameter);
        }

        void InitializeModule(ModuleInitializer<TGameContext> initializer, List<ModuleRecord> orderedModules)
        {
            var mip = new ModuleInitializationParameter(new GlobalModuleEntityInformation(rolesPerType, relationsPerType), serviceResolver);
            foreach (var mod in orderedModules)
            {
                if (mod.InitializedModule)
                {
                    continue;
                }


                try
                {
                    initializer.CurrentModuleId = mod.ModuleId;
                    mod.InitializedModule = true;
                    foreach (var mi in mod.ModuleInitializers)
                    {
                        mi(in mip, initializer);
                    }
                }
                finally
                {
                    initializer.CurrentModuleId = null;
                }
            }
        }

        void InitializeModuleContent(ModuleInitializer<TGameContext> initializer, List<ModuleRecord> orderedModules)
        {
            var mip = new ModuleInitializationParameter(new GlobalModuleEntityInformation(rolesPerType, relationsPerType), serviceResolver);
            foreach (var mod in orderedModules)
            {
                if (mod.InitializedContent)
                {
                    continue;
                }

                try
                {
                    initializer.CurrentModuleId = mod.ModuleId;

                    mod.InitializedContent = true;
                    foreach (var mi in mod.ContentInitializers)
                    {
                        mi(in mip, initializer);
                    }
                }
                finally
                {
                    initializer.CurrentModuleId = null;
                }
            }
        }

        void InitializeModuleRoles(ModuleInitializer<TGameContext> initializer,
                                   List<ModuleRecord> orderedModules)
        {
            foreach (var mod in orderedModules)
            {
                if (mod.InitializedRoles)
                {
                    continue;
                }

                try
                {
                    initializer.CurrentModuleId = mod.ModuleId;

                    mod.InitializedRoles = true;

                    foreach (var e in rolesPerType)
                    {
                        InitializeModuleRoleForType(initializer, e.Key, e.Value, mod);
                    }
                }
                finally
                {
                    initializer.CurrentModuleId = null;
                }
            }
        }

        void InitializeModuleRelations(ModuleInitializer<TGameContext> initializer,
                                       List<ModuleRecord> orderedModules)
        {
            foreach (var mod in orderedModules)
            {
                if (mod.InitializedRelations)
                {
                    continue;
                }

                try
                {
                    initializer.CurrentModuleId = mod.ModuleId;
                    mod.InitializedRelations = true;

                    foreach (var e in relationsPerType)
                    {
                        InitializeModuleRelationForType(initializer, e.Key, e.Value, mod);
                    }
                }
                finally
                {
                    initializer.CurrentModuleId = null;
                }
            }
        }

        class EntityRelationRecord
        {
            [UsedImplicitly] readonly Type entitySubject;
            readonly Dictionary<EntityRelation, HashSet<Type>> relations;

            public EntityRelationRecord(Type entitySubject)
            {
                this.entitySubject = entitySubject;
                this.relations = new Dictionary<EntityRelation, HashSet<Type>>();
            }

            public void RecordRelation(EntityRelation r, Type target)
            {
                if (!relations.TryGetValue(r, out var rs))
                {
                    rs = new HashSet<Type>();
                    relations[r] = rs;
                }

                rs.Add(target);
            }

            public IEnumerable<EntityRelation> Relations => relations.Keys;

            public bool TryQueryTarget(EntityRelation r, out IReadOnlyCollection<Type> result)
            {
                if (relations.TryGetValue(r, out var resultRaw))
                {
                    result = resultRaw;
                    return true;
                }

                result = default;
                return false;
            }
        }

        class EntityRoleRecord
        {
            [UsedImplicitly] readonly Type entityType;
            readonly HashSet<EntityRole> rolesPerType;

            public EntityRoleRecord(Type entityType)
            {
                this.entityType = entityType;
                this.rolesPerType = new HashSet<EntityRole>();
            }

            public void RecordRole(EntityRole s, string messageTemplateFragment = null, params object[] args)
            {
                if (rolesPerType.Contains(s))
                {
                    return;
                }

                if (Logger.IsEnabled(LogEventLevel.Debug))
                {
                    if (string.IsNullOrEmpty(messageTemplateFragment))
                    {
                        Logger.Debug("Entity {EntityType} requires role {Role}", entityType, s.Id);
                    }
                    else
                    {
                        // this is not performance critical code. Log readability trumps speed during setup.
                        var logArgs = new object[args.Length + 2];
                        logArgs[0] = entityType;
                        logArgs[1] = s.Id;
                        args.CopyTo(logArgs, 2);
                        Logger.Debug("Entity {EntityType} requires role {Role}" + messageTemplateFragment, logArgs);
                    }
                }

                rolesPerType.Add(s);
            }

            public bool RecordImpliedRelation(EntityRole s, EntityRole t, ModuleId declaringModule)
            {
                if (!rolesPerType.Contains(s))
                {
                    return false;
                }

                if (rolesPerType.Contains(t))
                {
                    return true;
                }

                if (Logger.IsEnabled(LogEventLevel.Debug))
                {
                    Logger.Debug("Entity {EntityType} requires role {Role} as role alias declared in {ModuleId}", entityType, t.Id, declaringModule);
                }

                rolesPerType.Add(t);
                return true;

            }

            public IEnumerable<EntityRole> Roles => rolesPerType;

            public bool HasRole(EntityRole role) => rolesPerType.Contains(role);

            public bool HasRole(EntityRole role, EntityRole requiredRole)
            {
                return rolesPerType.Contains(role) && rolesPerType.Contains(requiredRole);
            }
        }

        class ModuleRecord
        {
            public readonly ModuleId ModuleId;
            public readonly ModuleBase Module;
            public readonly List<ModuleRecord> Dependencies;
            public readonly List<ModuleInitializerDelegate<TGameContext>> ModuleInitializers;
            public readonly List<ModuleContentInitializerDelegate<TGameContext>> ContentInitializers;
            public bool IsUsedAsDependency { get; set; }
            public bool ResolvedRoles { get; set; }
            public bool ResolvedEquivalence { get; set; }
            public bool ResolvedOrder { get; set; }
            public bool InitializedModule { get; set; }
            public bool InitializedContent { get; set; }
            public bool InitializedRoles { get; set; }
            public bool InitializedRelations { get; set; }

            public int DependencyDepth
            {
                get
                {
                    if (Dependencies.Count == 0)
                    {
                        return 0;
                    }

                    return Dependencies.Max(d => d.DependencyDepth) + 1;
                }
            }

            public ModuleRecord(ModuleBase module)
            {
                Module = module;
                ModuleId = module.Id;
                Dependencies = new List<ModuleRecord>();
                ModuleInitializers = new List<ModuleInitializerDelegate<TGameContext>>();
                ContentInitializers = new List<ModuleContentInitializerDelegate<TGameContext>>();
            }

            public override string ToString()
            {
                return $"{nameof(ModuleId)}: {ModuleId}";
            }

            public void AddDependency(ModuleRecord value, string dependencyType)
            {
                foreach (var d in Dependencies)
                {
                    if (d.ModuleId == value.ModuleId)
                    {
                        return;
                    }
                }

                Dependencies.Add(value);
                Logger.Debug("Added Module dependency from {SourceModuleId} to {DependencyModuleId} as {DependencyType}",  ModuleId, value.ModuleId, dependencyType);
            }
        }
    }
}