using RogueEntity.Core.Infrastructure.Commands;

namespace RogueEntity.Core.Infrastructure.Actions.Idle
{
    public class IdleCommand: ICommand
    {
        readonly int turns;
        public const string ActionId = "Core.Actor.SkipTurn";

        public IdleCommand(int turns = 1)
        {
            this.turns = turns;
        }

        public int Turns => turns;

        public string Id => ActionId;
    }
}