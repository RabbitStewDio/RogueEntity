using Microsoft.Xna.Framework;
using RogueEntity.Core.Players;
using RogueEntity.Core.Utils;
using RogueEntity.SadCons.Controls;
using SadConsole;
using System;
using Console = SadConsole.Console;
using Rectangle = RogueEntity.Core.Utils.Rectangle;

namespace RogueEntity.SadCons
{
    public abstract class GameShell<TConsoleType>: IConsoleParentContext
        where TConsoleType: IConsoleContext
    {
        public event Action ConsoleResized;

        protected TConsoleType ControlsCanvas { get; set; }
        Console ParentConsole { get; set; }
        
        Dimension lastConsoleSize;

        public void Initialize() => Initialize(Global.CurrentScreen);

        public void Initialize(Console rootConsole)
        {
            this.ParentConsole = rootConsole;
            Settings.ResizeMode = Settings.WindowResizeOptions.None;
            SadConsoleHelper.SadConsoleGameInstance.WindowResized += OnWindowResized;
            lastConsoleSize = new Dimension(rootConsole.Width, rootConsole.Height);
            this.ParentConsole.Clear();

            InitializeLateOverride();

            Show();
        }
        
        public Rectangle Bounds => new Rectangle(ParentConsole.Position.X,
                                                 ParentConsole.Position.Y,
                                                 ParentConsole.Width,
                                                 ParentConsole.Height);

        protected virtual void InitializeLateOverride()
        { }

        void OnWindowResized(object sender, EventArgs e)
        {
            var fontSize = ParentConsole.Font.Size;
            ParentConsole.Resize(Global.WindowWidth / fontSize.X, Global.WindowHeight / fontSize.Y, false);
            lastConsoleSize = new Dimension(ParentConsole.Width, ParentConsole.Height);
            ConsoleResized?.Invoke();
        }

        public bool IsChildVisible(Console c)
        {
            return ParentConsole.Children.Contains(c);
        }

        public void SetChildVisible(Console c, bool state)
        {
            if (state)
            { 
                ParentConsole.Children.Add(c);
            }
            else
            {
                ParentConsole.Children.Remove(c);
            }
        }

        public bool IsVisible
        {
            get
            {
                return ControlsCanvas?.IsVisible ?? false;
            }
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
            ControlsCanvas?.Show();
        }

        public void Hide()
        {
            ControlsCanvas?.Hide();
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
