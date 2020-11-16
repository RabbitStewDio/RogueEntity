using EnTTSharp.Entities;

namespace RogueEntity.Api.Modules.Helpers
{
    public interface IModuleEntityInitializationCallback<TGameContext>
    {
        void PerformInitialization<TEntityId>(IModuleInitializationData<TGameContext, TEntityId> moduleContext)
            where TEntityId : IEntityKey;
    }
}