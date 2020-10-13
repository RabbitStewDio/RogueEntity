using System;

namespace ValionRL.Core.MapFragments
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