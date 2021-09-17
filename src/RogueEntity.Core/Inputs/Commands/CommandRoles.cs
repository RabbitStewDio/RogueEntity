using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Inputs.Commands
{
    public static class CommandRoles
    {
        public static EntityRole CreateRoleFor<TCommand>(CommandType<TCommand> cmd)
        {
            return new EntityRole($"Role.Core.Inputs.Commands.Executor[{cmd.CommandId.CommandId}]");
        }
    }
}
