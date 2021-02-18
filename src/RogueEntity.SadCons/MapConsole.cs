using EnTTSharp.Entities;
using RogueEntity.Core.Players;
using SadConsole;
using Console = SadConsole.Console;

namespace RogueEntity.SadCons
{
    public class MapConsole: ConsoleContext<Console>
    {
        readonly MapConsoleState sharedState;

        public MapConsole()
        {
            this.sharedState = new MapConsoleState();
            this.sharedState.PlayerId = new PlayerTag(PlayerIds.SinglePlayer);
        }

        public override void Initialize(IConsoleParentContext parentContext)
        {
            base.Initialize(parentContext);
            
            Console = new Console(parentContext.ScreenBounds.Width, parentContext.ScreenBounds.Height);
        }

        public PlayerTag PlayerId
        {
            get => sharedState.PlayerId;
            set => sharedState.PlayerId = value;
        }

        public void Initialize<TPlayerEntity>(IPlayerService<TPlayerEntity> playerService)
            where TPlayerEntity : IEntityKey
        {
            Console.Components.RemoveAll();
            Console.Components.Add(new MapConsoleMouseHandler<TPlayerEntity>(sharedState, playerService));
            Console.Components.Add(new MapConsoleKeyboardHandler<TPlayerEntity>(sharedState, playerService));
        }
    }
}
