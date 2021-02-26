using Microsoft.Xna.Framework;
using RogueEntity.SadCons;
using RogueEntity.Samples.BoxPusher.Core;
using SadConsole;
using SadConsole.Controls;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherGameUIContext : ConsoleContext<ControlsConsole>
    {
        readonly BoxPusherGame game;
        Label statusLabel;
        Window quitConfirmWindow;

        public BoxPusherGameUIContext(BoxPusherGame game)
        {
            this.game = game;
        }

        public override void Initialize(IConsoleParentContext parentContext)
        {
            base.Initialize(parentContext);

            Console = new ControlsConsole(parentContext.Bounds.Width, 1);
            Console.FocusOnMouseClick = false;
            Console.IsVisible = true;

            statusLabel = SadConsoleControls.CreateLabel("Position: [99, 99]").WithPlacementAt(0, 0);
            Console.Add(statusLabel);

            quitConfirmWindow = new Window(40, 20);
            quitConfirmWindow.Center();
            quitConfirmWindow.Add(SadConsoleControls.CreateLabel("Really Quit?").WithPlacementAt(5, 5));
            quitConfirmWindow.Add(SadConsoleControls.CreateButton("Quit", 10, 3).WithAction(OnBackToMenu).WithPlacementAt(4, 12));
            quitConfirmWindow.Add(SadConsoleControls.CreateButton("Cancel", 10, 3).WithAction(OnCancelQuit).WithPlacementAt(16, 12));
        }

        public void ShowQuitDialog()
        {
            quitConfirmWindow.Show(true);
        }

        protected override void OnParentConsoleResized()
        {
            base.OnParentConsoleResized();
            System.Console.WriteLine("System UI resized: " + ParentContext.Bounds);
            
            Console.Resize(ParentContext.Bounds.Width, 1, true, new Rectangle(0, 0, ParentContext.Bounds.Width, 1));
            
            quitConfirmWindow.Center();
        }

        void OnCancelQuit()
        {
            quitConfirmWindow.Hide();
        }

        void OnBackToMenu()
        {
            game.Stop();
            quitConfirmWindow.Hide();
        }
    }
}
