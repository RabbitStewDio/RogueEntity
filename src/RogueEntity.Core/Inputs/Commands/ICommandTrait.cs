using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Inputs.Commands
{
    public interface ICommandLift<TActorId, TResult>
    {
        TResult PerformCommandAction<TCommand>(TActorId k, TCommand cmd);
    }

    public interface ICommandTrait<TActorId> : IItemTrait
        where TActorId : IEntityKey
    {
        bool CanHandle<TCommand>();

        bool TryActionOn<TResult>(IItemResolver<TActorId> r, TActorId k, ICommandLift<TActorId, TResult> lifter, out TResult result);
    }

    public interface ICommandTrait<TActorId, TCommand> : ICommandTrait<TActorId>
        where TActorId : IEntityKey
    {
        bool IsCommandValidForState(TActorId actor);
        bool IsCommandValidForState(TActorId actor, TCommand cmd);
    }
}
