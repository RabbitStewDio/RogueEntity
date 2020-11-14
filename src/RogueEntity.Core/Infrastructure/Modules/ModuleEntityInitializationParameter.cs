using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Services;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public readonly struct ModuleEntityInitializationParameter<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        public readonly IModuleEntityInformation EntityInformation;
        public readonly IServiceResolver ServiceResolver;
        public readonly IModuleContentDeclarations<TGameContext, TEntityId> ContentDeclarations;

        public ModuleEntityInitializationParameter([NotNull] IModuleEntityInformation entityInformation,
                                                   [NotNull] IServiceResolver serviceResolver,
                                                   [NotNull] IModuleContentDeclarations<TGameContext, TEntityId> contentDeclarations)
        {
            EntityInformation = entityInformation ?? throw new ArgumentNullException(nameof(entityInformation));
            ServiceResolver = serviceResolver ?? throw new ArgumentNullException(nameof(serviceResolver));
            ContentDeclarations = contentDeclarations ?? throw new ArgumentNullException(nameof(contentDeclarations));
        }
    }
}