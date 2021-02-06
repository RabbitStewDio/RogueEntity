using Microsoft.Xna.Framework;
using RogueEntity.Core.Players;
using RogueEntity.Core.Utils;
using RogueEntity.SadCons.Controls;
using SadConsole;
using SadConsole.Controls;
using System;
using Console = SadConsole.Console;
using Rectangle = RogueEntity.Core.Utils.Rectangle;

namespace RogueEntity.SadCons
{
    public abstract class GameShell<TProfile> : IConsoleContext
    {
        public event Action ConsoleResized;

        protected ControlsConsole ControlsCanvas { get; private set; }
        protected Console ParentConsole { get; private set; }

        Dimension lastConsoleSize;

        public GameShell()
        {
        }

        public LoadGameContext<TProfile> LoadProfileContext { get; private set; }

        Console IConsoleContext.Console => ParentConsole;

        IConsoleContext IConsoleContext.ParentContext => null;

        public void Initialize() => Initialize(Global.CurrentScreen);

        public void Initialize(Console rootConsole)
        {
            this.ParentConsole = rootConsole;
            Settings.ResizeMode = Settings.WindowResizeOptions.None;
            SadConsoleHelper.SadConsoleGameInstance.WindowResized += OnWindowResized;
            lastConsoleSize = new Dimension(rootConsole.Width, rootConsole.Height);
            this.ParentConsole.Clear();

            var newGameButton = new Button(20, 3)
            {
                Text = "New Game",
                Position = new Point(2, 2)
            };
            newGameButton.Click += (e, args) => OnNewGame();

            var loadGameButton = new Button(20, 3)
            {
                Text = "Continue",
                Position = new Point(2, 7)
            };
            loadGameButton.Click += (e, args) => OnLoadGame();

            var quitGameButton = new Button(20, 3)
            {
                Text = "Quit",
                Position = new Point(2, 12)
            };
            quitGameButton.Click += (e, args) => OnQuitGame();


            ControlsCanvas = new ControlsConsole(24, 19);
            ControlsCanvas.Position = new Point(this.ParentConsole.Width - ControlsCanvas.Width - 2, this.ParentConsole.Height - ControlsCanvas.Height - 2);
            ControlsCanvas.Add(newGameButton);
            ControlsCanvas.Add(loadGameButton);
            ControlsCanvas.Add(quitGameButton);

            InitializeOverride();
            
            LoadProfileContext = new LoadGameContext<TProfile>(ProfileManager);
            LoadProfileContext.Initialize(this);

            InitializeLateOverride();

            Show();
        }
        
        protected abstract IPlayerProfileManager<TProfile> ProfileManager { get; }

        public Rectangle Bounds => new Rectangle(ParentConsole.Position.X,
                                                 ParentConsole.Position.Y,
                                                 ParentConsole.Width,
                                                 ParentConsole.Height);

        protected virtual void InitializeOverride()
        { }

        protected virtual void InitializeLateOverride()
        { }

        void OnWindowResized(object sender, EventArgs e)
        {
            var fontSize = ParentConsole.Font.Size;
            ParentConsole.Resize(Global.WindowWidth / fontSize.X, Global.WindowHeight / fontSize.Y, false);
            lastConsoleSize = new Dimension(ParentConsole.Width, ParentConsole.Height);
            ConsoleResized?.Invoke();
        }

        public bool IsVisible
        {
            get => ParentConsole.Children.Contains(ControlsCanvas);
            set
            {
                if (value == IsVisible)
                {
                    return;
                }

                if (value)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        public void Show()
        {
            ParentConsole.Children.Add(ControlsCanvas);
        }

        public void Hide()
        {
            ParentConsole.Children.Remove(ControlsCanvas);
        }

        public virtual void Update(GameTime time)
        { }

        public virtual void Draw(GameTime time)
        {
            var currentConsoleSize = new Dimension(ParentConsole.Width, ParentConsole.Height);
            if (lastConsoleSize != currentConsoleSize)
            {
                lastConsoleSize = currentConsoleSize;
                ConsoleResized?.Invoke();
            }

            ControlsCanvas.Position = new Point(this.ParentConsole.Width - ControlsCanvas.Width,
                                                this.ParentConsole.Height - ControlsCanvas.Height);
        }

        public virtual void Destroy()
        { }

        public virtual void OnNewGame()
        { }

        public virtual void OnLoadGame()
        {
            LoadProfileContext.IsVisible = true;
        }

        public void OnQuitGame()
        {
            SadConsole.Game.Instance.Exit();
        }
    }
}
