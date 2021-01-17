using System;

namespace RogueEntity.Generator.MapFragments
{
    [Flags]
    public enum MapFragmentMirror
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        Both = 3
    }
}