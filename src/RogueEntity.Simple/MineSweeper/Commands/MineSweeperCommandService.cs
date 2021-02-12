using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Inputs.Commands;

namespace RogueEntity.Simple.MineSweeper
{
    public class MineSweeperCommandService<TActor>
        where TActor : IEntityKey
    {
        readonly IItemResolver<TActor> itemResolver;

        public MineSweeperCommandService(IItemResolver<TActor> itemResolver)
        {
            this.itemResolver = itemResolver;
        }

        public bool IsActive(TActor actor)
        {
            return !itemResolver.IsDestroyed(actor);
        }

        public bool IsValid<TCommand>(TActor actor)
        {
            if (itemResolver.TryQueryTrait(actor, out ICommandTrait<TActor, TCommand> trait))
            {
                return trait.IsCommandValidForState(actor);
            }

            return false;
        }

        public bool IsValid<TCommand>(TActor actor, TCommand cmd)
        {
            if (itemResolver.TryQueryTrait(actor, out ICommandTrait<TActor, TCommand> trait))
            {
                return trait.IsCommandValidForState(actor, cmd);
            }

            return false;
        }

        public bool TrySubmit<TCommand>(TActor actor, TCommand cmd)
        {
            return itemResolver.TryUpdateData(actor, cmd, out _);
        }
    }
}
