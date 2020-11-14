using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Modules.Helpers
{
    public interface IEntitySystemDeclaration<TGameContext, TEntityId>: ISystemDeclaration
        where TEntityId : IEntityKey
    {
        public EntityRegistrationDelegate<TEntityId> EntityRegistration { get;  }
        public EntitySystemRegistrationDelegate<TGameContext, TEntityId> EntitySystemRegistration { get; }
    }
}