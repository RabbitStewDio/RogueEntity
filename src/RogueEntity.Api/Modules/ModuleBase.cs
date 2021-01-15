using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Utils;

namespace RogueEntity.Api.Modules
{
    public abstract class ModuleBase
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
        public ModuleId Id { get; protected set; }
        public string Name { get; protected set; }
        public string Author { get; protected set; }
        public string Description { get; protected set; }

        public IEnumerable<ModuleDependency> ModuleDependencies
        {
            get { return moduleDependencies; }
        }

        public ReadOnlyListWrapper<EntityRelation> RequiredRelations => requiredRelations;
        public ReadOnlyListWrapper<EntityRole> RequiredRoles => requiredRoles;

        public IEnumerable<DeclaredEntityRoleRecord> DeclaredEntityTypes => declaredRoles.Values;
        public IEnumerable<DeclaredEntityRelationRecord> DeclaredEntityRelations => declaredRelations.Values;

        public bool TryGetDeclaredRole<TEntityId>(out DeclaredEntityRoleRecord roleRecord)
        {
            return declaredRoles.TryGetValue(typeof(TEntityId), out roleRecord);
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
            where TEntityId : IEntityKey
        {
            if (declaredRoles.TryGetValue(typeof(TEntityId), out var rr))
            {
                rr.With(r);
            }
            else
            {
                rr = new DeclaredEntityRoleRecord(typeof(TEntityId), r, (cb, m) => cb.ActivateEntity<TEntityId>(m));
                declaredRoles[rr.EntityType] = rr;
            }

            return new DeclareDependencyBuilder(this, r);
        }

        [Obsolete]
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

        public virtual void ProcessDeclaredSystems<TEntityId>(in ModuleInitializationParameter p,
                                                              IModuleInitializationData<TEntityId> moduleContext,
                                                              IModuleInitializer initializer)
            where TEntityId : IEntityKey
        { }
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
