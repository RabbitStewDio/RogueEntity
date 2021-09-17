using System;
using System.Collections.Generic;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;

namespace RogueEntity.Api.Modules
{
    public abstract class ModuleBase: IModule
    {
        readonly List<ModuleDependency> moduleDependencies;
        readonly Dictionary<Type, DeclaredEntityRelationRecord> declaredRelations;

        readonly List<EntityRelation> requiredRelations;
        readonly List<EntityRole> requiredRoles;

        protected ModuleBase()
        {
            moduleDependencies = new List<ModuleDependency>();

            declaredRelations = new Dictionary<Type, DeclaredEntityRelationRecord>();

            requiredRelations = new List<EntityRelation>();
            requiredRoles = new List<EntityRole>();
        }

        public bool IsFrameworkModule { get; protected set; }
        public ModuleId Id { get; protected set; }
        public string Name { get; protected set; }
        public string Author { get; protected set; }
        public string Description { get; protected set; }

        public ReadOnlyListWrapper<ModuleDependency> ModuleDependencies => moduleDependencies;

        public bool HasRequiredRole(in EntityRole role) => RequiredRoles.Contains(role);
        public bool HasRequiredRelation(in EntityRelation relation) => RequiredRelations.Contains(relation);

        public ReadOnlyListWrapper<EntityRelation> RequiredRelations => requiredRelations;
        public ReadOnlyListWrapper<EntityRole> RequiredRoles => requiredRoles;

        public IEnumerable<DeclaredEntityRelationRecord> DeclaredEntityRelations => declaredRelations.Values;

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

        public DeclareDependencyBuilder DeclareRelation<TSubject, TObject>(EntityRelation r)
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

        public RequireDependencyBuilder WithRequiredRole(EntityRole r)
        {
            this.module.RequireRelation(new EntityRelation(ModuleRelationNames.ImpliedRoleRelationId, role, r, false));
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
        readonly ModuleBase module;
        readonly EntityRole role;

        public DeclareDependencyBuilder(ModuleBase module, EntityRole role)
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
