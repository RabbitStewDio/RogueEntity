using System;
using System.Collections.Generic;
using EnTTSharp.Entities;

namespace RogueEntity.Api.Modules.Helpers
{
    public interface IModuleInitializationData<TEntityId>: IModuleContentDeclarations<TEntityId>
        where TEntityId : IEntityKey
    {
        IEnumerable<IEntitySystemDeclaration<TEntityId>> EntitySystems { get; }
    }

    public interface IModuleInitializationData
    {
        IEnumerable<IGlobalSystemDeclaration> GlobalSystems { get; }
        IEnumerable<IGlobalSystemDeclaration> GlobalFinalizerSystems { get; }
        IEnumerable<(Type entityType, Action<IModuleEntityInitializationCallback> callback)> EntityInitializers { get; }
    }
}