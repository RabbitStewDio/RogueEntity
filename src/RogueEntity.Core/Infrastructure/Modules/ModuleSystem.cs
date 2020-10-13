using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public partial class ModuleSystem<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem<TGameContext>>();

        readonly IServiceResolver serviceResolver;
        readonly List<ModuleRecord> modulePool;
        readonly Dictionary<string, ModuleRecord> modulesById;
        readonly Dictionary<Type, Dictionary<EntityRole, ModuleRecord>> rolesPerType;
        readonly Dictionary<Type, Dictionary<EntityRelation, HashSet<Type>>> relationsPerType;
        bool initialized;

        public ModuleSystem(IServiceResolver serviceResolver = null)
        {
            modulesById = new Dictionary<string, ModuleRecord>();
            modulePool = new List<ModuleRecord>();

            this.serviceResolver = serviceResolver ?? new DefaultServiceResolver();

            rolesPerType = new Dictionary<Type, Dictionary<EntityRole, ModuleRecord>>();
            relationsPerType = new Dictionary<Type, Dictionary<EntityRelation, HashSet<Type>>>();
        }

        public void AddModule(ModuleBase module)
        {
            if (initialized)
            {
                throw new InvalidOperationException("Cannot add modules to an already initialized system");
            }

            if (modulesById.ContainsKey(module.Id))
            {
                return;
            }

            Logger.Debug("Registered module {ModuleId}", module.Id);

            var moduleRecord = new ModuleRecord(module);
            modulesById[module.Id] = moduleRecord;
            
            modulePool.Add(moduleRecord);
        }

        public void ScanForModules()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            foreach (var assembly in assemblies)
            {
                ScanForModules(assembly);
            }
        }

        public void ScanForModules(Assembly assembly)
        {
            foreach (var typeInfo in assembly.DefinedTypes)
            {
                if (!typeof(ModuleBase).IsAssignableFrom(typeInfo))
                {
                    continue;
                }

                if (typeInfo.IsAbstract || typeInfo.IsGenericType)
                {
                    continue;
                }

                if (Activator.CreateInstance(typeInfo) is ModuleBase module)
                {
                    AddModule(module);
                }
            }
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

            if (modulePool.Count == 0)
            {
                // No modules declared. Its OK if you dont want to use the module system.
                return;
            }

            PopulateModuleDependencies();

            var openModules = CollectRootLevelModules();

            CollectDeclaredRoles(openModules);
            ResolveEquivalenceRoles(openModules);

            var orderedModules = ComputeModuleOrder(openModules);
            Logger.Debug("Processing Modules in order: \n{Modules}", PrintModuleDependencyList(orderedModules));

            foreach (var mod in orderedModules)
            {
                if (mod.InitializedEntities)
                {
                    continue;
                }

                mod.InitializedEntities = true;
                InitializeModuleEntities(mod.Module, initializer);
            }

            foreach (var mod in orderedModules)
            {
                if (mod.InitializedContent)
                {
                    continue;
                }

                mod.InitializedContent = true;
                InitializeModuleSystems(mod.Module);
            }
        }

        void InitializeModuleEntities(ModuleBase module,
                                      IModuleInitializer<TGameContext> initializer)
        {
            CallModuleInitializer(module, initializer);

            foreach (var e in rolesPerType)
            {
                var entityType = e.Key;
                var roles = e.Value;
                foreach (var role in roles.Keys)
                {
                    if (!module.RequiredRoles.Contains(role))
                    {
                        continue;
                    }

                    CallRoleInitializer(entityType, module, initializer, role);
                }
            }

            foreach (var e in relationsPerType)
            {
                var entityType = e.Key;
                var relations = e.Value;
                foreach (var relation in relations)
                {
                    if (!module.RequiredRelations.Contains(relation.Key))
                    {
                        continue;
                    }

                    foreach (var targetType in relation.Value)
                    {
                        CallRelationInitializer(entityType, targetType, module, initializer, relation.Key);
                    }
                }
            }
        }

        void CallModuleInitializer(ModuleBase module, IModuleInitializer<TGameContext> initializer)
        {
            foreach (var m in module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var attr = m.GetCustomAttribute<ModuleInitializerAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (m.IsSameAction(typeof(IServiceResolver), typeof(IModuleInitializer<TGameContext>)))
                {
                    m.Invoke(module, new object[] {serviceResolver, initializer});
                    Logger.Verbose("Invoking module initializer {Method}", m);
                    continue;
                }

                if (!m.IsSameGenericAction(new[] {typeof(TGameContext)},
                                           out var genericMethod, out var errorHint,
                                           typeof(IServiceResolver), typeof(IModuleInitializer<TGameContext>)))
                {
                    if (!string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(errorHint);
                    }

                    throw new ArgumentException($"Expected a method with signature 'void XXX(IServiceResolver, IModuleInitializer<TGameContext>), but found {m} in module {module.Id}");
                }

                Logger.Verbose("Invoking module initializer {Method}", genericMethod);
                genericMethod.Invoke(module, new object[] {serviceResolver, initializer});
            }
        }

        void CallRoleInitializer(Type entityType, ModuleBase module, IModuleInitializer<TGameContext> initializer, EntityRole role)
        {
            foreach (var m in module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var attr = m.GetCustomAttribute<EntityRoleInitializerAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (attr.RoleName != role.Id)
                {
                    continue;
                }

                if (!m.IsSameGenericAction(new []{typeof(TGameContext), entityType}, 
                                           out var genericMethod, out var errorHint,
                                           typeof(IServiceResolver), typeof(IModuleInitializer<TGameContext>), typeof(EntityRole)))
                {
                    if (!string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(errorHint);
                    }
                    
                    throw new ArgumentException(
                        $"Expected a generic method with signature 'void XXX<TGameContext, TEntityId>(IServiceResolver, IModuleInitializer<TGameContext>, EntityRole), but found {m} in module {module.Id}");
                }

                Logger.Verbose("Invoking role initializer {Method}", genericMethod);
                genericMethod.Invoke(module, new object[] {serviceResolver, initializer, role});
            }
        }

        void CallRelationInitializer(Type subjectType, Type entityType, ModuleBase module, IModuleInitializer<TGameContext> initializer, EntityRelation relation)
        {
            foreach (var m in module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var attr = m.GetCustomAttribute<EntityRelationInitializerAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (attr.RelationName != relation.Id)
                {
                    continue;
                }

                if (!m.IsSameGenericAction(new[] {typeof(TGameContext), subjectType, entityType},
                                           out var genericMethod, out var errorHint,
                                           typeof(IServiceResolver), typeof(IModuleInitializer<TGameContext>), typeof(EntityRelation)))
                {
                    if (!string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(errorHint);
                    }

                    throw new ArgumentException(
                        $"Expected a generic method with signature 'void XXX<TGameContext, TSubjectEntityId, TEntityId>(IServiceResolver, IModuleInitializer<TGameContext>, EntityRelation), but found {m} in module {module.Id}");
                }

                Logger.Verbose("Invoking relation initializer {Method}", genericMethod);
                genericMethod.Invoke(module, new object[] {serviceResolver, initializer, relation});
            }
        }


        void InitializeModuleSystems(ModuleBase module)
        {
            
        }

        void PopulateModuleDependencies()
        {
            foreach (var m in modulePool)
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
            public readonly ModuleBase Module;
            public readonly List<ModuleRecord> Dependencies;
            public bool HasDependentModules { get; set; }
            public bool ResolvedRoles { get; set; }
            public bool ResolvedEquivalence { get; set; }
            public bool ResolvedOrder { get; set; }
            public bool InitializedEntities { get; set; }
            public bool InitializedContent { get; set; }

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
            }

            public override string ToString()
            {
                return $"{nameof(ModuleId)}: {ModuleId}";
            }
        }
    }
}