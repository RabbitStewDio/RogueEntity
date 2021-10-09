using EnTTSharp.Entities;
using RogueEntity.Api.Modules.Helpers;

namespace RogueEntity.Api.Modules
{
    public interface IModuleInitializer
    {
        IModuleContentContext<TEntityId> DeclareContentContext<TEntityId>()
            where TEntityId : IEntityKey;

        IModuleEntityContext<TEntityId> DeclareEntityContext<TEntityId>()
            where TEntityId : IEntityKey;

        void Register(EntitySystemId id, int priority, GlobalSystemRegistrationDelegate entityRegistration);

        void RegisterFinalizer(EntitySystemId id, int priority, GlobalSystemRegistrationDelegate entityRegistration);
    }
}
