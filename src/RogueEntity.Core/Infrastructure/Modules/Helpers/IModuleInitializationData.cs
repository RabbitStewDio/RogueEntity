using System;
using System.Collections.Generic;
using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Modules.Helpers
{
    public interface IModuleInitializationData<TGameContext, TEntityId>: IModuleContentDeclarations<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        IEnumerable<IEntitySystemDeclaration<TGameContext, TEntityId>> EntitySystems { get; }
    }

    public interface IModuleInitializationData<TGameContext>
    {
        IEnumerable<IGlobalSystemDeclaration<TGameContext>> GlobalSystems { get; }
        IEnumerable<(Type entityType, Action<IModuleEntityInitializationCallback<TGameContext>> callback)> EntityInitializers { get; }
    }
}