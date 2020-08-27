using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

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

        void Register(EntitySystemId id, int priority,
                      Action<EntityRegistry<TEntityId>> entityRegistration,
                      ModuleEntityContext<TGameContext, TEntityId>.SystemRegistrationDelegate systemRegistration = null);

        void Register(EntitySystemId id, int priority,
                      ModuleEntityContext<TGameContext, TEntityId>.SystemRegistrationDelegate systemRegistration = null);
    }
}