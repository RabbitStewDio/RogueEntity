using RogueEntity.Core.Runtime;
using RogueEntity.Core.Utils;
using RogueEntity.Samples.MineSweeper.Core;
using RogueEntity.Samples.MineSweeper.Core.Commands;
using RogueEntity.Samples.MineSweeper.Core.Services;
using SadConsole;
using SadConsole.Components;
using SadConsole.Input;

namespace RogueEntity.Samples.MineSweeper.MonoGame
{
    public class MineSweeperMouseHandler: MouseConsoleComponent
    {
        readonly MineSweeperInputState inputState;
        readonly MineSweeperGame game;
        readonly IMineSweeperGameParameterService gameParameters;

        public MineSweeperMouseHandler(MineSweeperInputState inputState, MineSweeperGame game)
        {
            this.inputState = inputState;
            this.game = game;
            this.gameParameters = game.GameParameterService;
        }

        public override void ProcessMouse(Console console, MouseConsoleState state, out bool handled)
        {
            if (!console.IsVisible || !state.IsOnConsole)
            {
                handled = false;
                return;
            }
            
            var rawPos = state.ConsoleCellPosition;
            if (!gameParameters.WorldParameter.ValidInputBounds.Contains(rawPos.X, rawPos.Y))
            {
                handled = false;
                return;
            }

            if (game.Status != GameStatus.Running)
            {
                handled = true;
                return;
            }
            
            var pos = new Position2D(rawPos.X, rawPos.Y);
            inputState.MouseHoverPosition = pos;
            
            if (state.Mouse.LeftClicked)
            {
                // indicate that the user wants to explore a cell
                System.Console.WriteLine("Submitting RevealMap at " + pos);
                game.CommandService.TrySubmit(game.PlayerData.Value.EntityId, new RevealMapPositionCommand(pos));
            }
            
            if (state.Mouse.RightClicked)
            {
                // indicate that the user wants to toggle a flag
                System.Console.WriteLine("Submitting ToggleFlag at " + pos);
                game.CommandService.TrySubmit(game.PlayerData.Value.EntityId, new ToggleFlagCommand(pos));
            }
            
            handled = true;

        }
    }
}
