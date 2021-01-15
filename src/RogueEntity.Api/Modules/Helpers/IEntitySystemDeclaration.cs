using EnTTSharp.Entities;

namespace RogueEntity.Api.Modules.Helpers
{
    public interface IEntitySystemDeclaration<TEntityId>: ISystemDeclaration
        where TEntityId : IEntityKey
    {
        public EntityRegistrationDelegate<TEntityId> EntityRegistration { get;  }
        public EntitySystemRegistrationDelegate<TEntityId> EntitySystemRegistration { get; }
    }
}