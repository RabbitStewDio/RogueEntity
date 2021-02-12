using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Inputs.Commands
{
    public interface ICommandTrait: IItemTrait
    {
        bool CanHandle<TCommand>();
    }
    
    public interface ICommandTrait<TActor, TCommand>: ICommandTrait
        where TActor : IEntityKey
    {
        bool IsCommandValidForState(TActor actor);
        bool IsCommandValidForState(TActor actor, TCommand cmd);
    }
}
