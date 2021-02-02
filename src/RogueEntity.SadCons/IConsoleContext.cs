using RogueEntity.Core.Utils;
using System;
using Console = SadConsole.Console;

namespace RogueEntity.SadCons
{
    public interface IConsoleContext
    {
        event Action ConsoleResized;

        bool IsVisible { get; set; }

        IConsoleContext ParentContext { get; }
        Console Console { get; }
        Rectangle Bounds { get; }

        void Show();
        void Hide();
    }
}
