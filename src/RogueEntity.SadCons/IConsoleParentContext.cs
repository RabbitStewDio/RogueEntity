using RogueEntity.Core.Utils;
using System;

namespace RogueEntity.SadCons
{
    public interface IConsoleParentContext
    {
        event Action ConsoleResized;
        Rectangle Bounds { get; }
        Rectangle ScreenBounds { get; }
    }
}
