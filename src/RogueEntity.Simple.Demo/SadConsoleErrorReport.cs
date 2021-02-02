using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
using System;

namespace RogueEntity.Simple
{
    public class SadConsoleErrorReport
    {
        const bool TriggerCrash = true;
        
        static void _Main()
        {
            // Setup the engine and create the main window.
            SadConsole.Game.Create(80, 25);

            var shell = new SadConsoleErrorReport();
            SadConsole.Game.OnInitialize += SetUp;

            // Start the game.
            SadConsole.Game.Instance.Run();
            SadConsole.Game.Instance.Dispose();
            
            // now resize the window to be larger
            // (The bug does not seem to trigger if you first make the window smaller!)
        }

        static void SetUp()
        {
            Settings.ResizeMode = Settings.WindowResizeOptions.None;
            
            var parentConsole = Global.CurrentScreen;
            var cc = new ControlsConsole(10, 10);
            cc.Position = new Point(5, 5);

            var showButton = new Button(5, 5)
            {
                Text = "Click",
                Position = new Point(1, 1)
            };
            cc.Add(showButton);
            parentConsole.Children.Add(cc);
            
            var w = new Window(parentConsole.Width - 10, parentConsole.Height - 10)
            {
                Position = new Point(5, 5)
            };

            showButton.Click += (s, e) =>
            {
                parentConsole.Children.Add(w);
                w.Show();
            };
            
            var button = new Button(10, 1)
            {
                Text = "Hello Crash!",
                Position = new Point(w.Width - 15, w.Height - 5)
            };
            w.Add(button);
            w.SetRenderCells();
            w.FillWithRandomGarbage();
            

            var game = SadConsole.Game.Instance as SadConsole.Game ?? throw new NullReferenceException();
            game.WindowResized += (s, e) =>
            {
                // first adjust the parent console to consume the whole screen
                var fontSize = parentConsole.Font.Size;
                parentConsole.Resize(Global.WindowWidth / fontSize.X, Global.WindowHeight / fontSize.Y, false);
              
                // now refloat the window.
                var width = parentConsole.Width - 10;
                var height = parentConsole.Height - 10;

                if (TriggerCrash)
                {
                    // This resize does not adjust the viewport of the scrolling console / window
                    w.Resize(width, height, true);
                }
                else
                {
                    // This one does, crudely, by forcing the user to come up with a sensible viewport value themselves.
                    w.Resize(width, height, true, new Rectangle(0, 0, width, height));
                }
                
                System.Console.WriteLine("Size: " + width + ", " + height);
                System.Console.WriteLine("ViewPort: " + w.ViewPort);

                button.Position = new Point(w.Width - 15, w.Width - 5);

                w.IsDirty = true;
            };

        }
    }
}
