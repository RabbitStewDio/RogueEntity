using JetBrains.Annotations;
using RogueEntity.Core.Utils;
using System;
using Console = SadConsole.Console;

namespace RogueEntity.SadCons
{
    public interface IConsoleParentContext
    {
        event Action ConsoleResized;
        Rectangle Bounds { get; }

        bool IsChildVisible(Console c);
        void SetChildVisible(Console c, bool state);
    }
    
    public abstract class ConsoleContext<TConsoleType> : IConsoleContext
        where TConsoleType : Console
    {
        public IConsoleParentContext ParentContext { get; private set; }
        public event Action ConsoleResized;

        public virtual void Initialize([NotNull] IConsoleParentContext parentContext)
        {
            this.ParentContext = parentContext ?? throw new ArgumentNullException(nameof(parentContext));
            this.ParentContext.ConsoleResized += OnParentConsoleResized;
        }

        public Rectangle Bounds
        {
            get
            {
                return Console == null
                    ? new Rectangle()
                    : new Rectangle(Console.Position.X,
                                    Console.Position.Y,
                                    Console.Width,
                                    Console.Height);
            }
        }


        protected void FireConsoleResized() => ConsoleResized?.Invoke();

        protected virtual void OnParentConsoleResized()
        {
            ConsoleResized?.Invoke();
        }

        protected TConsoleType Console { get; set; }
        Console IConsoleContext.Console => Console;

        public bool IsChildVisible(Console c)
        {
            return Console.Children.Contains(c);
        }

        public void SetChildVisible(Console c, bool visibleState)
        {
            if (visibleState)
            {
                Console.Children.Add(Console);
            }
            else
            {
                Console.Children.Remove(Console);
            }
        }

        public bool IsVisible
        {
            get => ParentContext.IsChildVisible(Console);
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
            ParentContext.SetChildVisible(Console, true);
        }

        public virtual void Hide()
        {
            ParentContext.SetChildVisible(Console, false);
        }
    }
}
