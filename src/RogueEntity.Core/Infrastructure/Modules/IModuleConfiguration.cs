using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public interface IModuleConfiguration
    {
        void DeclareDependency(ModuleDependency dependencies);
        void DeclareDependencies(params ModuleDependency[] dependencies);
        RequireDependencyBuilder RequireRelation(EntityRelation r);
        RequireDependencyBuilder RequireRole(EntityRole r);
        RequireDependencyBuilder ForRole(EntityRole r);
        RequireDependencyBuilder ForRelation(EntityRelation r);
    }
}