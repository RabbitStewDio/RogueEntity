using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using Serilog;
using Serilog.Events;

namespace RogueEntity.Api.Modules.Helpers
{
    public interface IModuleEntityRecord: IModuleEntityInformation
    {
        void RecordRole(EntityRole s, string messageTemplateFragment = null, params object[] args);
        bool RecordImpliedRelation(EntityRole s, EntityRole t, ModuleId declaringModule);
        void RecordRelation(EntityRelation r, Type target);
    }
    
    public class GlobalModuleEntityInformation : IModuleEntityInformation
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem>();
        readonly Dictionary<Type, ModuleEntityInformation> rolesPerType;

        public GlobalModuleEntityInformation()
        {
            this.rolesPerType = new Dictionary<Type, ModuleEntityInformation>();
        }

        public IModuleEntityRecord CreateEntityInformation<TEntityId>()
        {
            if (rolesPerType.TryGetValue(typeof(TEntityId), out var miRaw))
                return miRaw;
            
            var retval = new ModuleEntityInformation(this, typeof(TEntityId));
            rolesPerType[typeof(TEntityId)] = retval;
            return retval;
        }

        public bool TryGetModuleEntityInformation(Type entityId, out IModuleEntityInformation mi)
        {
            if (rolesPerType.TryGetValue(entityId, out var miRaw))
            {
                mi = miRaw;
                return true;
            }

            mi = default;
            return false;
        }
        
        public bool TryGetModuleEntityInformation<TEntityId>(out IModuleEntityInformation mi)
        {
            if (rolesPerType.TryGetValue(typeof(TEntityId), out var miRaw))
            {
                mi = miRaw;
                return true;
            }

            mi = default;
            return false;
        }

        public bool TryQueryRelationTarget(EntityRelation r, out IReadOnlyCollection<Type> result)
        {
            var roles = new HashSet<Type>();
            var resultIndicator = false;
            foreach (var rec in rolesPerType.Values)
            {
                if (rec.TryQueryRelationTarget(r, out var resultForType))
                {
                    resultIndicator = true;
                    roles.UnionWith(resultForType);
                }
            }

            result = roles;
            return resultIndicator;
        }

        public IEnumerable<EntityRole> Roles
        {
            get
            {
                var roles = new HashSet<EntityRole>();
                foreach (var r in rolesPerType.Values)
                {
                    roles.UnionWith(r.Roles);
                }

                return roles;
            }
        }

        public IEnumerable<EntityRelation> Relations
        {
            get
            {
                var roles = new HashSet<EntityRelation>();
                foreach (var r in rolesPerType.Values)
                {
                    roles.UnionWith(r.Relations);
                }

                return roles;
            }
        }

        public bool RoleExists(EntityRole role)
        {
            return HasRole(role);
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
            foreach (var t in rolesPerType.Values)
            {
                if (!HasRole(role))
                {
                    continue;
                }

                if (t.HasRelation(role, requiredRelation))
                {
                    return true;
                }
            }
            
            return false;
        }

        public IEnumerable<Type> FindEntityTypeForRole(EntityRole role)
        {
            foreach (var e in rolesPerType)
            {
                if (e.Value.HasRole(role))
                {
                    yield return e.Key;
                }
            }
        }

        public bool RecordImpliedRelation(EntityRole s, EntityRole t, ModuleId declaringModule)
        {
            var result = false;
            foreach (var e in rolesPerType)
            {
                result |= e.Value.RecordImpliedRelation(s, t, declaringModule);
            }

            return result;
        }

        public bool IsValidRole<TEntityId>(ModuleEntityRoleInitializerInfo< TEntityId> r, EntityRole role)
            where TEntityId : IEntityKey
        {
            if (rolesPerType.TryGetValue(typeof(TEntityId), out var rec))
            {
                return rec.IsValidRole(r, role);
            }

            return false;
        }
        
        public EntityRelation[] ResolveRelationsById(string[] relationNames)
        {
            if (relationNames == null || relationNames.Length == 0)
            {
                return new EntityRelation[0];
            }

            var l = new List<EntityRelation>();
            foreach (var r in relationNames)
            {
                foreach (var e in rolesPerType.Values)
                {
                    if (e.TryGetRelationById(r, out var relation))
                    {
                        l.Add(relation);
                    }
                }
            }

            return l.ToArray();
        }
        
        class ModuleEntityInformation : IModuleEntityRecord
        {
            [UsedImplicitly] readonly Type entitySubject;
            readonly IModuleEntityInformation globalInformation;
            readonly EntityRoleRecord rolesPerType;
            readonly EntityRelationRecord relationRecord;

            public ModuleEntityInformation([NotNull] IModuleEntityInformation globalInformation,
                                           [NotNull] Type entitySubject)
            {
                this.globalInformation = globalInformation ?? throw new ArgumentNullException(nameof(globalInformation));
                this.entitySubject = entitySubject ?? throw new ArgumentNullException(nameof(entitySubject));
                this.rolesPerType = new EntityRoleRecord(entitySubject);
                this.relationRecord = new EntityRelationRecord(entitySubject);
            }

            public IEnumerable<EntityRole> Roles => rolesPerType.Roles;

            public IEnumerable<EntityRelation> Relations => relationRecord.Relations;

            public bool TryGetRelationById(string relationName, out EntityRelation r)
            {
                foreach (var relation in relationRecord.Relations)
                {
                    if (relation.Id == relationName)
                    {
                        r = relation;
                        return true;
                    }
                }

                r = default;
                return false;
            }
            
            public void RecordRole(EntityRole s, string messageTemplateFragment = null, params object[] args)
            {
                rolesPerType.RecordRole(s, messageTemplateFragment, args);
            }

            public bool RecordImpliedRelation(EntityRole s, EntityRole t, ModuleId declaringModule)
            {
                return rolesPerType.RecordImpliedRelation(s, t, declaringModule);
            }

            public void RecordRelation(EntityRelation r, Type target)
            {
                relationRecord.RecordRelation(r, target);
            }

            public bool RoleExists(EntityRole role)
            {
                return globalInformation.RoleExists(role);
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

            public bool IsValidRole<TEntityId>(ModuleEntityRoleInitializerInfo< TEntityId> r, EntityRole role)
                where TEntityId : IEntityKey
            {
                if (r.Role != role)
                {
                    return false;
                }

                foreach (var requiredRole in r.RequiredRolesAnywhereInSystem)
                {
                    if (!RoleExists(requiredRole))
                    {
                        return false;
                    }
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

            public bool TryQueryRelationTarget(EntityRelation r, out IReadOnlyCollection<Type> result)
            {
                return relationRecord.TryQueryTarget(r, out result);
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
                
    }
}