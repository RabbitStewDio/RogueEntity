using Microsoft.Xna.Framework;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Runtime;
using RogueEntity.Generator.Commands;
using RogueEntity.SadCons;
using RogueEntity.Samples.BoxPusher.Core;
using RogueEntity.Samples.BoxPusher.Core.Commands;
using SadConsole;
using SadConsole.Controls;
using Serilog;
using System;
using System.ComponentModel;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherGameUIContext : ConsoleContext<ControlsConsole>
    {
        readonly ILogger Logger = SLog.ForContext<BoxPusherGameUIContext>();
        readonly BoxPusherGame game;
        Label statusLabel;
        Window quitConfirmWindow;
        Window winConfirmWindow;
        Button quitButton;
        Button resetButton;
        Button nextLevel;
        Button previousLevel;
        BoxPusherPlayerStatusService statusService;

        public BoxPusherGameUIContext(BoxPusherGame game)
        {
            this.game = game;
            this.game.GameInitialized += OnGameInitialized;
            if (game.Status == GameStatus.Initialized)
            {
                OnGameInitialized(this, EventArgs.Empty);
            }
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            statusService = this.game.StatusService;
            statusService.PropertyChanged += OnStatusChanged;
            statusService.LevelCleared += OnLevelCleared;
        }

        void OnLevelCleared(object sender, EventArgs e)
        {
            ShowWinDialog();
        }

        void OnStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BoxPusherPlayerStatusService.Position))
            {
                var pos = statusService.Position;
                if (pos.IsInvalid)
                {
                    statusLabel.DisplayText = "";
                    statusLabel.IsDirty = true;
                }
                else
                {
                    statusLabel.DisplayText = $"Position: [{pos.GridX,3},{pos.GridY,3}] Level: [{pos.GridZ,3}]";
                    statusLabel.IsDirty = true;
                }
            }

            if (e.PropertyName == nameof(BoxPusherPlayerStatusService.CurrentLevel))
            {
                // bonus: Enable and disable next/prev buttons based on the current and available levels.
            }
        }

        public override void Initialize(IConsoleParentContext parentContext)
        {
            base.Initialize(parentContext);

            var size = ParentContext.Bounds.BoundsInCells();
            Console = new ControlsConsole(size.Width, 2);
            Console.FocusOnMouseClick = false;
            Console.IsVisible = true;

            statusLabel = SadConsoleControls.CreateLabel("Position: [  -,  -] Level: [  -]").WithPlacementAt(0, 0);
            Console.Add(statusLabel);

            quitButton = SadConsoleControls.CreateButton("Quit", 10, 1).WithPlacementAt(2, 1).WithAction(ShowQuitDialog);
            Console.Add(quitButton);

            resetButton = SadConsoleControls.CreateButton("Reset", 10, 1).WithPlacementAt(14, 1).WithAction(OnResetLevel);
            Console.Add(resetButton);

            previousLevel = SadConsoleControls.CreateButton("<<", 10, 1).WithPlacementAt(26, 1).WithAction(OnPreviousLevel);
            Console.Add(previousLevel);

            nextLevel = SadConsoleControls.CreateButton(">>", 10, 1).WithPlacementAt(38, 1).WithAction(OnNextLevel);
            Console.Add(nextLevel);

            quitConfirmWindow = new Window(40, 20);
            // quitConfirmWindow.Parent = Console;
            quitConfirmWindow.Center();
            quitConfirmWindow.Add(SadConsoleControls.CreateLabel("Really Quit?").WithPlacementAt(5, 5));
            quitConfirmWindow.Add(SadConsoleControls.CreateButton("Quit", 10, 3).WithAction(OnBackToMenu).WithPlacementAt(4, 12));
            quitConfirmWindow.Add(SadConsoleControls.CreateButton("Cancel", 10, 3).WithAction(OnCancelDialogs).WithPlacementAt(16, 12));

            winConfirmWindow = new Window(40, 20);
            // winConfirmWindow.Parent = Console;
            winConfirmWindow.Center();
            winConfirmWindow.Add(SadConsoleControls.CreateLabel("Level Complete").WithPlacementAt(5, 5));
            winConfirmWindow.Add(SadConsoleControls.CreateButton("Next", 10, 3).WithAction(OnNextLevel).WithPlacementAt(4, 12));
            winConfirmWindow.Add(SadConsoleControls.CreateButton("Quit", 10, 3).WithAction(OnBackToMenu).WithPlacementAt(16, 12));
            winConfirmWindow.Add(SadConsoleControls.CreateButton("Cancel", 10, 3).WithAction(OnCancelDialogs).WithPlacementAt(28, 12));
        }

        void OnResetLevel()
        {
            if (game.PlayerData.TryGetValue(out var player))
            {
                if (game.CommandService.TrySubmit(player.EntityId, new ResetLevelCommand()))
                {
                    Logger.Debug("Requested restart of current level");
                }
                else
                {
                    Logger.Debug("Failed to request level reset");
                }
            }
        }

        void OnNextLevel()
        {
            if (game.PlayerData.TryGetValue(out var player) &&
                game.CurrentPlayerProfile.TryGetValue(out var profile))
            {
                var changeLevelCommand = new ChangeLevelCommand(profile.CurrentLevel + 1);
                if (game.CommandService.TrySubmit(player.EntityId, changeLevelCommand))
                {
                    Logger.Debug("Requested start of next level {Level}", changeLevelCommand.Level);
                }
                else
                {
                    Logger.Debug("Failed to request next level change to {Level}", changeLevelCommand.Level);
                }
            }

            winConfirmWindow.Hide();
        }

        void OnPreviousLevel()
        {
            if (game.PlayerData.TryGetValue(out var player) &&
                game.CurrentPlayerProfile.TryGetValue(out var profile))
            {
                var changeLevelCommand = new ChangeLevelCommand(profile.CurrentLevel - 1);
                if (game.CommandService.TrySubmit(player.EntityId, changeLevelCommand))
                {
                    Logger.Debug("Requested start of previous level {Level}", changeLevelCommand.Level);
                }
                else
                {
                    Logger.Debug("Failed to request previous level change to {Level}", changeLevelCommand.Level);
                }
            }

            winConfirmWindow.Hide();
        }

        public void ShowWinDialog()
        {
            Logger.Debug("Showing win dialog");
            quitConfirmWindow.Hide();
            winConfirmWindow.Show(true);
        }

        public void ShowQuitDialog()
        {
            winConfirmWindow.Hide();
            quitConfirmWindow.Show(true);
        }

        protected override void OnParentConsoleResized()
        {
            base.OnParentConsoleResized();
            Logger.Debug("System UI resized to {Bounds} ", ParentContext.Bounds);

            var size = ParentContext.Bounds.BoundsInCells();
            Console.Resize(size.Width, 1, true, new Rectangle(0, 0, size.Width, 1));

            quitConfirmWindow.Center();
            winConfirmWindow.Center();
        }

        void OnCancelDialogs()
        {
            quitConfirmWindow.Hide();
            winConfirmWindow.Hide();
        }

        void OnBackToMenu()
        {
            game.DeactivatePlayer();
            game.Stop();
            winConfirmWindow.Hide();
            quitConfirmWindow.Hide();
        }
    }
}
