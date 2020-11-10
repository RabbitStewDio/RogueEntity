using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules.Initializers;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public partial class ModuleSystem<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem<TGameContext>>();

        readonly IServiceResolver serviceResolver;
        readonly List<ModuleRecord> contentModulePool;
        readonly Dictionary<string, ModuleRecord> modulesById;
        readonly Dictionary<Type, EntityRoleRecord> rolesPerType;
        readonly Dictionary<Type, EntityRelationRecord> relationsPerType;
        bool initialized;

        public ModuleSystem([NotNull] IServiceResolver serviceResolver)
        {
            modulesById = new Dictionary<string, ModuleRecord>();
            contentModulePool = new List<ModuleRecord>();

            this.serviceResolver = serviceResolver ?? throw new ArgumentNullException(nameof(serviceResolver));

            rolesPerType = new Dictionary<Type, EntityRoleRecord>();
            relationsPerType = new Dictionary<Type, EntityRelationRecord>();
        }

        public void Initialize(TGameContext context,
                               IModuleInitializer<TGameContext> initializer = null)
        {
            if (initialized)
            {
                return;
            }

            initializer ??= new ModuleInitializer<TGameContext>();

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

            if (contentModulePool.Count == 0)
            {
                // No content modules declared. Its OK, you apparently dont want to use the module system.
                return;
            }

            PopulateModuleDependencies();

            var openModules = CollectRootLevelModules();
            var orderedModules = ComputeModuleOrder(openModules);
            Logger.Debug("Processing Modules in order: \n{Modules}", PrintModuleDependencyList(orderedModules));

            // 1. Setup global functions. Those provide global services or configurations etc, and only depend on the context object. 
            InitializeModule(initializer, orderedModules);

            // 2. Initialize content. This sets up EntityDeclarations and thus tells the system which roles are used by each entity key
            //    encountered by the code.
            InitializeModuleContent(initializer, orderedModules);

            // 3. Based on the module information gathered, we can now resolve all roles and relations. First, some sorting. 
            CollectDeclaredRoles(orderedModules);
            ResolveEquivalenceRoles(orderedModules);

            // 4. Now start initializing trait systems for each entity role.
            InitializeModuleRoles(initializer, orderedModules);
            InitializeModuleRelations(initializer, orderedModules);
        }

        void InitializeModule(IModuleInitializer<TGameContext> initializer, List<ModuleRecord> orderedModules)
        {
            foreach (var mod in orderedModules)
            {
                if (mod.InitializedModule)
                {
                    continue;
                }

                mod.InitializedModule = true;
                foreach (var mi in mod.ModuleInitializers)
                {
                    mi(serviceResolver, initializer);
                }
            }
        }

        void InitializeModuleContent(IModuleInitializer<TGameContext> initializer, List<ModuleRecord> orderedModules)
        {
            foreach (var mod in orderedModules)
            {
                if (mod.InitializedContent)
                {
                    continue;
                }

                mod.InitializedContent = true;
                foreach (var mi in mod.ContentInitializers)
                {
                    mi(serviceResolver, initializer);
                }
            }
        }

        void InitializeModuleRoles(IModuleInitializer<TGameContext> initializer,
                                   List<ModuleRecord> orderedModules)
        {
            foreach (var mod in orderedModules)
            {
                if (mod.InitializedRoles)
                {
                    continue;
                }

                mod.InitializedRoles = true;

                foreach (var e in rolesPerType)
                {
                    InitializeModuleRoleForType(initializer, e.Key, e.Value, mod);
                }
            }
        }

        void InitializeModuleRelations(IModuleInitializer<TGameContext> initializer,
                                       List<ModuleRecord> orderedModules)
        {
            foreach (var mod in orderedModules)
            {
                if (mod.InitializedRelations)
                {
                    continue;
                }

                mod.InitializedRelations = true;

                foreach (var e in relationsPerType)
                {
                    InitializeModuleRelationForType(initializer, e.Key, e.Value, mod);
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

            public void Record(EntityRelation r, Type target)
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

            public void Record(EntityRole s)
            {
                if (!rolesPerType.Contains(s))
                {
                    rolesPerType.Add(s);
                }
            }

            public bool RecordRelation(EntityRole s, EntityRole t)
            {
                if (rolesPerType.Contains(s))
                {
                    if (!rolesPerType.Contains(t))
                    {
                        rolesPerType.Add(t);
                    }

                    return true;
                }

                return false;
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
            public readonly string ModuleId;
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

            public void AddDependency(ModuleRecord value)
            {
                foreach (var d in Dependencies)
                {
                    if (d.ModuleId == value.ModuleId)
                    {
                        return;
                    }
                }

                Dependencies.Add(value);
                Logger.Debug("Added Module dependency. " + ModuleId + " -> " + value.ModuleId);
            }
        }
    }
}