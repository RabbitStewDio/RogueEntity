using EnTTSharp.Entities;

namespace RogueEntity.Api.Modules.Helpers
{
    public interface IModuleEntityInitializationCallback
    {
        void PerformInitialization<TEntityId>(IModuleInitializationData<TEntityId> moduleContext)
            where TEntityId : IEntityKey;
    }
}