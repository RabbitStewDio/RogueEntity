using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Modules.Helpers
{
    public interface IModuleEntityInitializationCallback<TGameContext>
    {
        void PerformInitialization<TEntityId>(IModuleInitializationData<TGameContext, TEntityId> moduleContext)
            where TEntityId : IEntityKey;
    }
}