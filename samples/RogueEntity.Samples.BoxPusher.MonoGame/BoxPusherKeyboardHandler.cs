using RogueEntity.Samples.BoxPusher.Core;
using SadConsole;
using SadConsole.Components;
using SadConsole.Input;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherKeyboardHandler : KeyboardConsoleComponent
    {
        readonly BoxPusherGame game;
        readonly BoxPusherInputState sharedState;

        public BoxPusherKeyboardHandler(BoxPusherGame game, BoxPusherInputState sharedState)
        {
            this.game = game;
            this.sharedState = sharedState;
        }

        
        public override void ProcessKeyboard(Console console, Keyboard info, out bool handled)
        {
            handled = false;
        }
    }
}
