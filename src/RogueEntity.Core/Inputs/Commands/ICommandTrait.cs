using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Inputs.Commands
{
    public interface ICommandTrait<TActorId> : IItemTrait
        where TActorId : struct, IEntityKey
    {
        CommandTypeId CommandId { get; }
        
        bool CanHandle<TCommand>();

        bool TryRemoveCompletedCommandData(IItemResolver<TActorId> r, TActorId k);
    }

    public interface ICommandTrait<TActorId, TCommand> : ICommandTrait<TActorId>
        where TActorId : struct, IEntityKey
    {
        bool IsCommandValidForState(TActorId actor);
        bool IsCommandValidForState(TActorId actor, TCommand cmd);
    }
}
