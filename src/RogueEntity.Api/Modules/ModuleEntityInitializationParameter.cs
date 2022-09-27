using System;
using EnTTSharp.Entities;
using RogueEntity.Api.Services;

namespace RogueEntity.Api.Modules
{
    public readonly struct ModuleEntityInitializationParameter<TEntityId>
        where TEntityId : struct, IEntityKey
    {
        public readonly IModuleEntityInformation EntityInformation;
        public readonly IServiceResolver ServiceResolver;
        public readonly IModuleContentDeclarations<TEntityId> ContentDeclarations;

        public ModuleEntityInitializationParameter(IModuleEntityInformation entityInformation,
                                                   IServiceResolver serviceResolver,
                                                   IModuleContentDeclarations<TEntityId> contentDeclarations)
        {
            EntityInformation = entityInformation ?? throw new ArgumentNullException(nameof(entityInformation));
            ServiceResolver = serviceResolver ?? throw new ArgumentNullException(nameof(serviceResolver));
            ContentDeclarations = contentDeclarations ?? throw new ArgumentNullException(nameof(contentDeclarations));
        }
    }
}