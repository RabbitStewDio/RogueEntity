using System;
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
        IEnumerable<IGlobalSystemDeclaration<TGameContext>> GlobalSystems { get; }
        IEnumerable<(Type entityType, Action<IModuleEntityInitializationCallback<TGameContext>> callback)> EntityInitializers { get; }
    }

    public interface IModuleInitializationData<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        IEnumerable<(ModuleId, IBulkItemDeclaration<TGameContext, TEntityId>)> DeclaredBulkItems { get; }
        IEnumerable<(ModuleId, IReferenceItemDeclaration<TGameContext, TEntityId>)> DeclaredReferenceItems { get; }
        IEnumerable<IEntitySystemDeclaration<TGameContext, TEntityId>> EntitySystems { get; }
    }

    public interface ISystemDeclaration
    {
        ModuleId DeclaringModule { get; }
        EntitySystemId Id { get; }
        int Priority { get; }
        int InsertionOrder { get; }
    }
    
    public interface IEntitySystemDeclaration<TGameContext, TEntityId>: ISystemDeclaration
        where TEntityId : IEntityKey
    {
        public EntityRegistrationDelegate<TEntityId> EntityRegistration { get;  }
        public EntitySystemRegistrationDelegate<TGameContext, TEntityId> EntitySystemRegistration { get; }
    }
    
    public interface IGlobalSystemDeclaration<TGameContext>: ISystemDeclaration
    {
        GlobalSystemRegistrationDelegate<TGameContext> SystemRegistration { get; }
    }
}