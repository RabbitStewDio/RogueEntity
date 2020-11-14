using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Modules.Services;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public readonly struct ModuleEntityInitializationParameter<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        public readonly IModuleEntityInformation EntityInformation;
        public readonly IServiceResolver ServiceResolver;
        public readonly IModuleContentDeclarations<TGameContext, TEntityId> ContentDeclarations;

        public ModuleEntityInitializationParameter(IModuleEntityInformation entityInformation,
                                                   IServiceResolver serviceResolver,
                                                   IModuleContentDeclarations<TGameContext, TEntityId> contentDeclarations)
        {
            EntityInformation = entityInformation;
            ServiceResolver = serviceResolver;
            ContentDeclarations = contentDeclarations;
        }
    }
}