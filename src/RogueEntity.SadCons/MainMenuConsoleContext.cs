using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
using System;

namespace RogueEntity.SadCons
{
    public class MainMenuConsoleContext : ConsoleContext<ControlsConsole>
    {
        public event Action OnNewGame;
        public event Action OnLoadGame;
        public event Action OnSettings;
        public event Action OnQuitGame;
        
        public bool HasLoadScreen;
        public bool HasSettings;

        public override void Initialize(IConsoleParentContext parentContext)
        {
            base.Initialize(parentContext);

            Console = new ControlsConsole(24, 19);
            Console.Position = new Point(this.ParentContext.Bounds.Width - Console.Width - 2,
                                         this.ParentContext.Bounds.Height - Console.Height - 2);
            Console.Add(CreateNewGameButton());

            if (HasLoadScreen)
            {
                Console.Add(CreateLoadGameButton());
            }

            if (HasSettings)
            {
                Console.Add(CreateOptionsButton());
            }

            Console.Add(CreateQuitGameButton());
        }

        protected override void OnParentConsoleResized()
        {
            Console.Position = new Point(this.ParentContext.Bounds.Width - Console.Width,
                                         this.ParentContext.Bounds.Height - Console.Height);
            base.OnParentConsoleResized();
        }

        Button CreateQuitGameButton()
        {
            var quitGameButton = new Button(20, 3)
            {
                Text = "Quit",
                Position = new Point(2, 2 + Console.Children.Count * 5)
            };
            quitGameButton.Click += (e, args) => OnQuitGame?.Invoke();
            return quitGameButton;
        }

        Button CreateLoadGameButton()
        {
            var loadGameButton = new Button(20, 3)
            {
                Text = "Continue",
                Position = new Point(2, 2 + Console.Children.Count * 5)
            };
            loadGameButton.Click += (e, args) => OnLoadGame?.Invoke();
            return loadGameButton;
        }

        Button CreateNewGameButton()
        {
            var newGameButton = new Button(20, 3)
            {
                Text = "New Game",
                Position = new Point(2, 2 + Console.Children.Count * 5)
            };
            newGameButton.Click += (e, args) => OnNewGame?.Invoke();
            return newGameButton;
        }

        Button CreateOptionsButton()
        {
            var newGameButton = new Button(20, 3)
            {
                Text = "Settings",
                Position = new Point(2, 2 + Console.Children.Count * 5)
            };
            newGameButton.Click += (e, args) => OnSettings?.Invoke();
            return newGameButton;
        }

    }
}
