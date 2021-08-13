using RogueEntity.Core.Utils;
using SadConsole;
using System;
using Console = SadConsole.Console;

namespace RogueEntity.SadCons
{
    public abstract class ConsoleContext<TConsoleType> : IConsoleContext
        where TConsoleType : Console
    {
        public IConsoleParentContext ParentContext { get; private set; }
        public event Action ConsoleResized;

        public virtual void Initialize(IConsoleParentContext parentContext)
        {
            this.ParentContext = parentContext ?? throw new ArgumentNullException(nameof(parentContext));
            this.ParentContext.ConsoleResized += OnParentConsoleResized;
        }

        public ConsoleSize ScreenBounds => ParentContext.ScreenBounds;

        public ConsoleSize Bounds
        {
            get
            {
                if (Console is ContainerConsole || Console == null)
                {
                    // ContainerConsole has not size and therefore does not return sane values
                    return ParentContext.Bounds;
                }


                var pixelArea = Console.AbsoluteArea;
                var bounds = new Rectangle(pixelArea.X,
                                           pixelArea.Y,
                                           pixelArea.Width,
                                           pixelArea.Height);
                var fontSize = Console.Font.Size;
                return new ConsoleSize(bounds, new Dimension(fontSize.X, fontSize.Y));
            }
        }

        public void AddChildContext(IConsoleContext c)
        {
            c.Initialize(this);
            var console = c.Console;
            if (console is Window)
            {
                // ignore. Windows are free floating
            }
            else if (console != null)
            {
                Console.Children.Add(console);
            }
        }

        public void RemoveChildContext(IConsoleContext c)
        {
            var console = c.Console;
            if (console is Window)
            {
                // ignore. Windows are free floating
            }
            else if (console != null)
            {
                Console.Children.Remove(console);
            }
        }

        protected void FireConsoleResized() => ConsoleResized?.Invoke();

        protected virtual void OnParentConsoleResized()
        {
            ConsoleResized?.Invoke();
        }

        public TConsoleType Console { get; protected set; }
        Console IConsoleContext.Console => Console;

        public bool IsChildVisible(Console c)
        {
            return Console.Children.Contains(c);
        }

        public bool IsVisible
        {
            get => Console.IsVisible;
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
            if (Console is Window w)
            {
                // SadConsole has some questionable choices some times.
                w.Show(true);
            }
            else
            {
                Console.IsVisible = true;
            }
        }

        public virtual void Hide()
        {
            if (Console is Window w)
            {
                // SadConsole has some questionable choices some times.
                w.Hide();
            }
            else
            {
                Console.IsVisible = false;
            }
        }
    }
}
