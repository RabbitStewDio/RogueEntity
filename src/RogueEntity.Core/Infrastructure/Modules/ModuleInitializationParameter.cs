using RogueEntity.Core.Infrastructure.Modules.Services;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public readonly struct ModuleInitializationParameter
    {
        public readonly IModuleEntityInformation EntityInformation;
        public readonly IServiceResolver ServiceResolver;

        public ModuleInitializationParameter(IModuleEntityInformation entityInformation, IServiceResolver serviceResolver)
        {
            EntityInformation = entityInformation;
            ServiceResolver = serviceResolver;
        }
    }
}