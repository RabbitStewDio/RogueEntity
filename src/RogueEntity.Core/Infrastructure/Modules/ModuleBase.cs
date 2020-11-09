using System;
using System.Collections.Generic;
using System.Linq;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public static class ModuleRelationNames
    {
        public const string ImpliedRoleRelationId = "Relation.System.Implies";
    }

    public interface IModuleConfiguration
    {
        void DeclareDependency(ModuleDependency dependencies);
        void DeclareDependencies(params ModuleDependency[] dependencies);
        RequireDependencyBuilder RequireRelation(EntityRelation r);
        RequireDependencyBuilder RequireRole(EntityRole r);
        RequireDependencyBuilder ForRole(EntityRole r);
        RequireDependencyBuilder ForRelation(EntityRelation r);
    }

    public abstract class ModuleBase: IModuleConfiguration
    {
        readonly List<ModuleDependency> moduleDependencies;
        readonly Dictionary<Type, DeclaredEntityRoleRecord> declaredRoles;
        readonly Dictionary<Type, DeclaredEntityRelationRecord> declaredRelations;

        readonly List<EntityRelation> requiredRelations;
        readonly List<EntityRole> requiredRoles;

        protected ModuleBase()
        {
            moduleDependencies = new List<ModuleDependency>();

            declaredRoles = new Dictionary<Type, DeclaredEntityRoleRecord>();
            declaredRelations = new Dictionary<Type, DeclaredEntityRelationRecord>();

            requiredRelations = new List<EntityRelation>();
            requiredRoles = new List<EntityRole>();
        }

        public bool IsFrameworkModule { get; protected set; }
        public string Id { get; protected set; }
        public string Name { get; protected set; }
        public string Author { get; protected set; }
        public string Description { get; protected set; }

        public IEnumerable<ModuleDependency> ModuleDependencies
        {
            get { return moduleDependencies; }
        }

        public ReadOnlyListWrapper<EntityRelation> RequiredRelations => requiredRelations;
        public ReadOnlyListWrapper<EntityRole> RequiredRoles => requiredRoles;

        public IEnumerable<Type> DeclaredEntityTypes => declaredRoles.Keys.Concat(declaredRelations.Keys).Distinct();

        public bool TryGetEntityRecord(Type subject, out DeclaredEntityRoleRecord r)
        {
            return declaredRoles.TryGetValue(subject, out r);
        }

        public bool TryGetRelationRecord(Type subject, out DeclaredEntityRelationRecord r)
        {
            return declaredRelations.TryGetValue(subject, out r);
        }

        public bool TryGetRelationById(string id, out EntityRelation relation)
        {
            foreach (var r in requiredRelations)
            {
                if (r.Id == id)
                {
                    relation = r;
                    return true;
                }
            }

            foreach (var r in declaredRelations.Values)
            {
                if (r.TryGetRelationById(id, out relation))
                {
                    return true;
                }
            }

            relation = default;
            return false;
        }

        protected DeclareDependencyBuilder DeclareEntity<TEntityId>(EntityRole r)
        {
            if (declaredRoles.TryGetValue(typeof(TEntityId), out var rr))
            {
                rr = rr.With(r);
            }
            else
            {
                rr = new DeclaredEntityRoleRecord(typeof(TEntityId), r);
                declaredRoles[rr.EntityType] = rr;
            }

            return new DeclareDependencyBuilder(this, r);
        }

        protected DeclareDependencyBuilder DeclareRelation<TSubject, TObject>(EntityRelation r)
        {
            if (!declaredRelations.TryGetValue(typeof(TSubject), out var record))
            {
                record = new DeclaredEntityRelationRecord(typeof(TSubject));
            }

            record.Declare(typeof(TObject), r);

            declaredRelations[typeof(TSubject)] = record;
            return new DeclareDependencyBuilder(this, r.Subject);
        }

        public RequireDependencyBuilder RequireRole(EntityRole r)
        {
            if (!requiredRoles.Contains(r))
            {
                requiredRoles.Add(r);
            }

            return new RequireDependencyBuilder(this, r);
        }

        public RequireDependencyBuilder ForRole(EntityRole r)
        {
            return new RequireDependencyBuilder(this, r);
        }

        public RequireDependencyBuilder RequireRelation(EntityRelation r)
        {
            if (!requiredRelations.Contains(r))
            {
                requiredRelations.Add(r);
            }

            return new RequireDependencyBuilder(this, r.Subject);
        }

        public RequireDependencyBuilder ForRelation(EntityRelation r)
        {
            return new RequireDependencyBuilder(this, r.Subject);
        }

        public void DeclareDependency(ModuleDependency dependencies)
        {
            if (!moduleDependencies.Contains(dependencies))
            {
                moduleDependencies.Add(dependencies);
            }
        }

        public void DeclareDependencies(params ModuleDependency[] dependencies)
        {
            moduleDependencies.AddRange(dependencies);
        }
    }

    public readonly struct RequireDependencyBuilder
    {
        readonly ModuleBase module;
        readonly EntityRole role;

        public RequireDependencyBuilder(ModuleBase module, EntityRole role)
        {
            this.module = module;
            this.role = role;
        }

        public RequireDependencyBuilder WithImpliedRole(EntityRole r)
        {
            this.module.RequireRelation(new EntityRelation(ModuleRelationNames.ImpliedRoleRelationId, role, r, true));
            return this;
        }

        public RequireDependencyBuilder WithDependencyOn(string moduleId)
        {
            module.DeclareDependency(ModuleDependency.Of(moduleId));
            return this;
        }
    }

    public readonly struct DeclareDependencyBuilder
    {
        readonly IModuleConfiguration module;
        readonly EntityRole role;

        public DeclareDependencyBuilder(IModuleConfiguration module, EntityRole role)
        {
            this.module = module;
            this.role = role;
        }

        public DeclareDependencyBuilder WithImpliedRole(EntityRole r)
        {
            // dont register dependency roles so that we can check the module setup later.
            this.module.RequireRelation(new EntityRelation(ModuleRelationNames.ImpliedRoleRelationId, role, r));
            return this;
        }

        public DeclareDependencyBuilder WithImpliedRole(EntityRole r, ModuleDependency moduleId)
        {
            // dont register dependency roles so that we can check the module setup later.
            this.module.RequireRelation(new EntityRelation(ModuleRelationNames.ImpliedRoleRelationId, role, r));
            this.module.DeclareDependency(moduleId);

            return this;
        }

        public DeclareDependencyBuilder WithDependencyOn(string moduleId)
        {
            module.DeclareDependency(ModuleDependency.Of(moduleId));
            return this;
        }
    }
}