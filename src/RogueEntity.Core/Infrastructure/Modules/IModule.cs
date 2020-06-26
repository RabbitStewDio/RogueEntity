using System.Collections.Generic;
using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public interface IModule<TGameContext>
    {
        string Id { get; }
        string Name { get; }
        string Author { get; }
        string Description { get; }

        IEnumerable<string> ModuleDependencies { get; }
        void Initialize(TGameContext context, 
                        IModuleInitializer<TGameContext> initializer);
    }

    public interface IModuleInitializer<TGameContext>
    {
        IModuleEntityContext<TGameContext, TEntityId> DeclareEntityContext<TEntityId>() where TEntityId : IEntityKey;
    }

    public interface IModuleEntityContext<TGameContext, TEntityId> where TEntityId : IEntityKey
    {
        IEnumerable<IBulkItemDeclaration<TGameContext, TEntityId>> DeclaredBulkItems { get; }
        IEnumerable<IReferenceItemDeclaration<TGameContext, TEntityId>> DeclaredReferenceItems { get; }
        IEnumerable<IEntitySystemFactory<TGameContext, TEntityId>> EntitySystems { get; }

        ItemDeclarationId Declare(IBulkItemDeclaration<TGameContext, TEntityId> item);
        ItemDeclarationId Declare(IReferenceItemDeclaration<TGameContext, TEntityId> item);
    }
}