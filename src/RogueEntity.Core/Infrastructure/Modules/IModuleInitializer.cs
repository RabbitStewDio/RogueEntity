using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Modules.Helpers;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public interface IModuleInitializer<TGameContext>
    {
        IModuleContentContext<TGameContext, TEntityId> DeclareContentContext<TEntityId>()
            where TEntityId : IEntityKey;
        
        IModuleEntityContext<TGameContext, TEntityId> DeclareEntityContext<TEntityId>()
            where TEntityId : IEntityKey;

        void Register(EntitySystemId id,
                      int priority,
                      GlobalSystemRegistrationDelegate<TGameContext> entityRegistration);
    }
}