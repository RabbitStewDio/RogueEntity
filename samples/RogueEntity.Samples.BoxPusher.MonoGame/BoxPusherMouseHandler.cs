using RogueEntity.Samples.BoxPusher.Core;
using SadConsole;
using SadConsole.Components;
using SadConsole.Input;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherMouseHandler : MouseConsoleComponent
    {
        readonly BoxPusherGame game;
        readonly BoxPusherInputState sharedState;

        public BoxPusherMouseHandler(BoxPusherGame game, BoxPusherInputState sharedState)
        {
            this.game = game;
            this.sharedState = sharedState;
        }

        public override void ProcessMouse(Console console, MouseConsoleState state, out bool handled)
        {
            handled = false;
        }
    }
}
