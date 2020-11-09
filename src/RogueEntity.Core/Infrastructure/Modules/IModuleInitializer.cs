using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public interface IModuleInitializer<TGameContext>
    {
        IModuleEntityContext<TGameContext, TEntityId> DeclareEntityContext<TEntityId>()
            where TEntityId : IEntityKey;

        void Register(EntitySystemId id,
                      int priority,
                      GlobalSystemRegistrationDelegate<TGameContext> entityRegistration);
    }

    public interface IModuleInitializationData<TGameContext>
    {
        IEnumerable<IGlobalSystemRegistration<TGameContext>> GlobalSystems { get; }
    }

    public interface IModuleInitializationData<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        IEnumerable<IBulkItemDeclaration<TGameContext, TEntityId>> DeclaredBulkItems { get; }
        IEnumerable<IReferenceItemDeclaration<TGameContext, TEntityId>> DeclaredReferenceItems { get; }
        IEnumerable<IEntitySystemDeclaration<TGameContext, TEntityId>> EntitySystems { get; }
    }
    
    public interface IEntitySystemDeclaration<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        string DeclaringModule { get; }
        EntitySystemId Id { get; }
        int Priority { get; }
        int InsertionOrder { get; }

        public EntityRegistrationDelegate<TEntityId> EntityRegistration { get;  }
        public EntitySystemRegistrationDelegate<TGameContext, TEntityId> EntitySystemRegistration { get; }

    }
    
    public interface IGlobalSystemRegistration<TGameContext>
    {
        string DeclaringModule { get; }
        EntitySystemId Id { get; }
        int Priority { get; }
        int InsertionOrder { get; }
        
        GlobalSystemRegistrationDelegate<TGameContext> SystemRegistration { get; }
    }
}