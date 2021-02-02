using JetBrains.Annotations;
using RogueEntity.Core.Utils;
using System;
using Console = SadConsole.Console;

namespace RogueEntity.SadCons
{
    public abstract class ConsoleContext<TConsoleType> : IConsoleContext
        where TConsoleType : Console
    {
        public IConsoleContext ParentContext { get; private set; }
        public event Action ConsoleResized;

        public virtual void Initialize([NotNull] IConsoleContext parentContext)
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
        { }

        protected TConsoleType Console { get; set; }
        Console IConsoleContext.Console => Console;

        public bool IsVisible
        {
            get => ParentContext.Console.Children.Contains(Console);
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
            ParentContext.Console.Children.Add(Console);
        }

        public virtual void Hide()
        {
            ParentContext.Console.Children.Remove(Console);
        }
    }
}
