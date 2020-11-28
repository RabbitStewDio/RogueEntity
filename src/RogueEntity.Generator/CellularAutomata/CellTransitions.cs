using System;

namespace RogueEntity.Generator.CellularAutomata
{
    [Flags]
    public enum CellTransitions
    {
        Unchanged = 0,
        Born = 1,
        Dead = 2
    }
}