using Microsoft.Xna.Framework;
using RogueEntity.Core.Utils;
using RogueEntity.SadCons.Controls;
using SadConsole;
using System;
using Console = SadConsole.Console;
using Rectangle = RogueEntity.Core.Utils.Rectangle;

namespace RogueEntity.SadCons
{
    public abstract class GameShell<TConsoleType>: IConsoleParentContext
        where TConsoleType: class, IConsoleContext
    {
        public event Action ConsoleResized;

        protected TConsoleType ControlsCanvas { get; private set; }
        protected Console ParentConsole { get; private set; }
        
        Dimension lastConsoleSize;

        public void Initialize() => Initialize(Global.CurrentScreen);

        public void Initialize(Console rootConsole)
        {
            this.ParentConsole = rootConsole;
            Settings.ResizeMode = Settings.WindowResizeOptions.None;
            SadConsoleHelper.SadConsoleGameInstance.WindowResized += OnWindowResized;
            lastConsoleSize = new Dimension(rootConsole.Width, rootConsole.Height);
            this.ParentConsole.Clear();

            ControlsCanvas = InitializeLateOverride();
            this.ParentConsole.Children.Add(ControlsCanvas.Console);

            Show();
        }

        public Rectangle ScreenBounds => Bounds;

        public Rectangle Bounds => new Rectangle(ParentConsole.Position.X,
                                                 ParentConsole.Position.Y,
                                                 ParentConsole.Width,
                                                 ParentConsole.Height);

        protected abstract TConsoleType InitializeLateOverride();

        void OnWindowResized(object sender, EventArgs e)
        {
            var fontSize = ParentConsole.Font.Size;
            ParentConsole.Resize(Global.WindowWidth / fontSize.X, Global.WindowHeight / fontSize.Y, false);
            lastConsoleSize = new Dimension(ParentConsole.Width, ParentConsole.Height);
            ConsoleResized?.Invoke();
        }


        public bool IsVisible
        {
            get => ControlsCanvas.IsVisible;
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


        public virtual void Show()
        {
            ControlsCanvas.IsVisible = true;
        }

        public virtual void Hide()
        {
            ControlsCanvas.IsVisible = false;
        }

        public virtual void Update(GameTime time)
        {
        }

        public virtual void Draw(GameTime time)
        {
            var currentConsoleSize = new Dimension(ParentConsole.Width, ParentConsole.Height);
            if (lastConsoleSize != currentConsoleSize)
            {
                lastConsoleSize = currentConsoleSize;
                ConsoleResized?.Invoke();
            }
        }

        public virtual void Destroy()
        { }
    }
}
