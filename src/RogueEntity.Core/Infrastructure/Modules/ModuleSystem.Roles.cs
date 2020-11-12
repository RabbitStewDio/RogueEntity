using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public partial class ModuleSystem<TGameContext>
    {
        void InitializeModuleRoleForType(IModuleInitializer<TGameContext> initializer, Type entityType, EntityRoleRecord roles, ModuleRecord mod)
        {
            relationsPerType.TryGetValue(entityType, out var relationRecord);
            var mi = new ModuleEntityInformation(entityType, roles, relationRecord);
            var moduleInitializerParams = new ModuleInitializationParameter(mi, serviceResolver);
            foreach (var role in roles.Roles)
            {
                foreach (var roleInitializer in CollectRoleInitializers(entityType, mod.Module, mi, role))
                {
                    if (mi.IsValidRole(roleInitializer, role))
                    {
                        roleInitializer.Initializer(in moduleInitializerParams, initializer, role);
                    }
                }
            }
        }

        List<ModuleEntityRoleInitializerInfo<TGameContext>> CollectRoleInitializers(Type entityType, ModuleBase module, IModuleEntityInformation mi, EntityRole role)
        {
            var retval = new List<ModuleEntityRoleInitializerInfo<TGameContext>>();

            var methodInfos = module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var m in methodInfos)
            {
                var attr = m.GetCustomAttribute<InitializerCollectorAttribute>();
                if (attr == null || attr.Type != InitializerCollectorType.Roles)
                {
                    continue;
                }

                if (!m.IsSameGenericFunction(new[] {typeof(TGameContext), entityType},
                                             out var genericMethod, out var errorHint,
                                             typeof(IEnumerable<ModuleEntityRoleInitializerInfo<TGameContext>>),
                                             typeof(IServiceResolver), typeof(IModuleEntityInformation), typeof(EntityRole)))
                {
                    if (string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(
                            $"Expected a generic method with signature 'void XXX<TGameContext, TEntityId>(IServiceResolver, IModuleInitializer<TGameContext>, EntityRole), but found {m} in module {module.Id}");
                    }

                    Logger.Information("Generic constraints on module {Module} with method {Method} do not match. {errorHint}", module.Id, m, errorHint);
                    continue;
                }

                if (genericMethod.Invoke(module, new object[] {serviceResolver, mi, role}) is IEnumerable<ModuleEntityRoleInitializerInfo<TGameContext>> list)
                {
                    retval.AddRange(list);
                }
            }

            foreach (var m in methodInfos)
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

                if (!m.IsSameGenericAction(new[] {typeof(TGameContext), entityType},
                                           out var genericMethod, out var errorHint,
                                           typeof(ModuleInitializationParameter).MakeByRefType(), typeof(IModuleInitializer<TGameContext>), typeof(EntityRole)))
                {
                    if (string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(
                            $"Expected a generic method with signature 'void XXX<TGameContext, TEntityId>(ModuleInitializationParameter ByRef, IModuleInitializer<TGameContext>, EntityRole), but found {m} in module {module.Id}");
                    }

                    Logger.Information("Generic constraints on module {Module} with method {Method} do not match. {errorHint}", module.Id, m, errorHint);
                    continue;
                }

                Logger.Verbose("Invoking role initializer {Method}", genericMethod);
                var initializer = (ModuleEntityRoleInitializerDelegate<TGameContext>)
                    Delegate.CreateDelegate(typeof(ModuleEntityRoleInitializerDelegate<TGameContext>), module, genericMethod);
                retval.Add(ModuleEntityRoleInitializerInfo.CreateFor(role, initializer)
                                                          .WithRequiredRoles(attr.ConditionalRoles.Select(e => new EntityRole(e)).ToArray())
                                                          .WithRequiredRelations(ResolveRelationsById(module, attr.ConditionalRelations))
                );
            }

            return retval;
        }

        void InitializeModuleRelationForType(IModuleInitializer<TGameContext> initializer, Type entityType, EntityRelationRecord relations, ModuleRecord mod)
        {
            rolesPerType.TryGetValue(entityType, out var roles);
            var mi = new ModuleEntityInformation(entityType, roles, relations);
            var moduleInitializerParams = new ModuleInitializationParameter(mi, serviceResolver);

            foreach (var relation in relations.Relations)
            {
                // check precondition - is there no relation data available? ... then skip
                if (!relations.TryQueryTarget(relation, out var targetTypes))
                {
                    continue;
                }

                foreach (var targetType in targetTypes)
                foreach (var roleInitializer in CollectRelationInitializers(entityType, targetType, mod.Module, mi, relation))
                {
                    if (mi.IsValidRelation(roleInitializer, relation))
                    {
                        roleInitializer.Initializer(in moduleInitializerParams, initializer, relation);
                    }
                }
            }
        }

        List<ModuleEntityRelationInitializerInfo<TGameContext>> CollectRelationInitializers(Type entityType, Type subjectType, ModuleBase module, IModuleEntityInformation mi, EntityRelation relation)
        {
            var retval = new List<ModuleEntityRelationInitializerInfo<TGameContext>>();

            var methodInfos = module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var m in methodInfos)
            {
                var attr = m.GetCustomAttribute<InitializerCollectorAttribute>();
                if (attr == null || attr.Type != InitializerCollectorType.Relations)
                {
                    continue;
                }

                if (!m.IsSameGenericFunction(new[] {typeof(TGameContext), subjectType, entityType},
                                             out var genericMethod, out var errorHint,
                                             typeof(IEnumerable<ModuleEntityRelationInitializerInfo<TGameContext>>),
                                             typeof(IServiceResolver), typeof(IModuleEntityInformation), typeof(EntityRelation)))
                {
                    if (string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(
                            $"Expected a generic method with signature 'void XXX<TGameContext, TEntityId>(IServiceResolver, IModuleInitializer<TGameContext>, EntityRole), but found {m} in module {module.Id}");
                    }

                    Logger.Information("Generic constraints on module {Module} with method {Method} do not match. {errorHint}", module.Id, m, errorHint);
                    continue;
                }

                if (genericMethod.Invoke(module, new object[] {serviceResolver, mi, relation}) is IEnumerable<ModuleEntityRelationInitializerInfo<TGameContext>> list)
                {
                    retval.AddRange(list);
                }
            }

            foreach (var m in methodInfos)
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
                                           typeof(ModuleInitializationParameter).MakeByRefType(), typeof(IModuleInitializer<TGameContext>), typeof(EntityRelation)))
                {
                    if (string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(
                            $"Expected a generic method with signature 'void XXX<TGameContext, TEntityId>(ModuleInitializationParameter ByRef, IModuleInitializer<TGameContext>, EntityRole), but found {m} in module {module.Id}");
                    }

                    Logger.Information("Generic constraints on module {Module} with method {Method} do not match. {errorHint}", module.Id, m, errorHint);
                    continue;
                }

                Logger.Verbose("Invoking role initializer {Method}", genericMethod);
                var initializer = (ModuleEntityRelationInitializerDelegate<TGameContext>)
                    Delegate.CreateDelegate(typeof(ModuleEntityRelationInitializerDelegate<TGameContext>), module, genericMethod);
                retval.Add(ModuleEntityRelationInitializerInfo.CreateFor(relation, initializer)
                                                              .WithRequiredSubjectRoles(attr.ConditionalSubjectRoles.Select(e => new EntityRole(e)).ToArray())
                                                              .WithRequiredTargetRoles(attr.ConditionalObjectRoles.Select(e => new EntityRole(e)).ToArray())
                );
            }

            return retval;
        }

        EntityRelation[] ResolveRelationsById(ModuleBase module, string[] relationNames)
        {
            if (relationNames == null || relationNames.Length == 0)
            {
                return new EntityRelation[0];
            }

            var l = new List<EntityRelation>();
            foreach (var r in relationNames)
            {
                if (module.TryGetRelationById(r, out var relation))
                {
                    l.Add(relation);
                }
            }

            return l.ToArray();
        }

        
        class ItemTypeRegistrationHandler : IModuleEntityInitializationCallback<TGameContext>
        {
            readonly IServiceResolver serviceResolver;
            public readonly Dictionary<EntityRoleInstance, Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)>> Roles;
            public readonly Dictionary<EntityRelationInstance, Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)>> Relations;


            public ItemTypeRegistrationHandler(IServiceResolver serviceResolver)
            {
                this.serviceResolver = serviceResolver;
                this.Roles = new Dictionary<EntityRoleInstance, Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)>>();
                this.Relations = new Dictionary<EntityRelationInstance, Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)>>();
            }

            public void PerformInitialization<TEntityId>(IModuleInitializationData<TGameContext, TEntityId> moduleContext)
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
                    RegisterTraitDependencies(Roles, i.GetEntityRoles(), i.Id, m);
                    RegisterTraitDependencies(Relations, i.GetEntityRelations(), i.Id, m);
                    ctx.ItemRegistry.Register(i);
                }
            }

            void RegisterTraitDependencies<TKey>(Dictionary<TKey, Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)>> dataStore, 
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
        }

        class GlobalModuleEntityInformation : IModuleEntityInformation
        {
            readonly Dictionary<Type, EntityRoleRecord> rolesPerType;
            readonly Dictionary<Type, EntityRelationRecord> relationsPerType;

            public GlobalModuleEntityInformation(Dictionary<Type, EntityRoleRecord> rolesPerType, 
                                                 Dictionary<Type, EntityRelationRecord> relationsPerType)
            {
                this.rolesPerType = rolesPerType;
                this.relationsPerType = relationsPerType;
            }

            public bool HasRole(EntityRole role, EntityRole requiredRole)
            {
                foreach (var r in rolesPerType.Values)
                {
                    if (r.HasRole(role, requiredRole))
                    {
                        return true;
                    }
                }

                return false;
            }
            
            public bool HasRole(EntityRole role)
            {
                foreach (var r in rolesPerType.Values)
                {
                    if (r.HasRole(role))
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool HasRelation(EntityRole role, EntityRelation requiredRelation)
            {
                if (!HasRole(role)) return false;

                foreach (var r in relationsPerType.Values)
                {
                    if (r.TryQueryTarget(requiredRelation, out _))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        class ModuleEntityInformation : IModuleEntityInformation
        {
            [UsedImplicitly] readonly Type entitySubject;
            readonly EntityRoleRecord rolesPerType;
            readonly EntityRelationRecord relationRecord;

            public ModuleEntityInformation(Type entitySubject, EntityRoleRecord rolesPerType, EntityRelationRecord relationRecord = null)
            {
                this.entitySubject = entitySubject;
                this.rolesPerType = rolesPerType ?? new EntityRoleRecord(entitySubject);
                this.relationRecord = relationRecord ?? new EntityRelationRecord(entitySubject);
            }

            public bool HasRole(EntityRole role, EntityRole requiredRole)
            {
                return rolesPerType.HasRole(role, requiredRole);
            }

            public bool HasRole(EntityRole role)
            {
                return rolesPerType.HasRole(role);
            }

            public bool HasRelation(EntityRole role, EntityRelation requiredRelation)
            {
                if (!rolesPerType.HasRole(role))
                {
                    return false;
                }

                return relationRecord.TryQueryTarget(requiredRelation, out _);
            }

            public bool IsValidRole(ModuleEntityRoleInitializerInfo<TGameContext> r, EntityRole role)
            {
                if (r.Role != role)
                {
                    return false;
                }

                foreach (var requiredRole in r.RequiredRoles)
                {
                    if (!HasRole(role, requiredRole))
                    {
                        return false;
                    }
                }

                foreach (var requiredRelation in r.RequiredRelations)
                {
                    if (!HasRelation(role, requiredRelation))
                    {
                        return false;
                    }
                }

                return true;
            }

            public bool IsValidRelation(ModuleEntityRelationInitializerInfo<TGameContext> r, EntityRelation role)
            {
                if (r.Relation != role)
                {
                    return false;
                }

                foreach (var requiredRole in r.RequiredObjectRoles)
                {
                    if (!HasRole(role.Object, requiredRole))
                    {
                        return false;
                    }
                }

                foreach (var requiredRelation in r.RequiredSubjectRoles)
                {
                    if (!HasRole(role.Subject, requiredRelation))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public static List<(string, List<string>)> CreateTraitEvidence(Dictionary<ItemDeclarationId, (ModuleId, List<ItemTraitId>)> backend)
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