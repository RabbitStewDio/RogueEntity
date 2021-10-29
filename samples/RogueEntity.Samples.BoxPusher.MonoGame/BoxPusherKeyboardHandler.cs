using Microsoft.Xna.Framework.Input;
using RogueEntity.Core.Movement.GridMovement;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Samples.BoxPusher.Core;
using SadConsole;
using SadConsole.Components;
using Keyboard = SadConsole.Input.Keyboard;

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
            handled = true;
            
            if (sharedState.PlayerObserver.TryGetValue(out var observer) &&
                !observer.Position.IsInvalid)
            {
                if (TryGetInputDirection(info, out var d))
                {
                    var targetPos = EntityGridPosition.From(observer.Position) + d.ToCoordinates();
                    var cmd = new GridMoveCommand(targetPos);
                    if (!game.CommandService.TrySubmit(game.PlayerData.Value.EntityId, cmd))
                    {
                        System.Console.WriteLine("Unable to move to " + targetPos);
                    }
                }
            }

            if (info.IsKeyPressed(Keys.Escape))
            {
                sharedState.NotifyQuitInitiated();
            }
        }

        bool TryGetInputDirection(Keyboard info, out Direction d)
        {
            if (info.IsKeyPressed(Keys.Up) ||
                info.IsKeyPressed(Keys.NumPad8))
            {
                d = Direction.Up;
                return true;
            }
            
            if (info.IsKeyPressed(Keys.Down) ||
                info.IsKeyPressed(Keys.NumPad2))
            {
                d = Direction.Down;
                return true;
            }
            
            if (info.IsKeyPressed(Keys.Left) ||
                info.IsKeyPressed(Keys.NumPad4))
            {
                d = Direction.Left;
                return true;
            }
            
            if (info.IsKeyPressed(Keys.Right) ||
                info.IsKeyPressed(Keys.NumPad6))
            {
                d = Direction.Right;
                return true;
            }

            d = default;
            return false;
        }
    }
}
