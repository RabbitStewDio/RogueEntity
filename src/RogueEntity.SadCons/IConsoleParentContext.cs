using System;

namespace RogueEntity.SadCons
{
    public interface IConsoleParentContext
    {
        event Action ConsoleResized;
        ConsoleSize Bounds { get; }
        ConsoleSize ScreenBounds { get; }
    }
}
