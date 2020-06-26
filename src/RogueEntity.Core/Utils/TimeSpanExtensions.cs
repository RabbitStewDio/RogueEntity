using System;

namespace RogueEntity.Core.Utils
{
    public static class TimeSpanExtensions
    {
        public static long Divide(this TimeSpan d, TimeSpan f)
        {
            return d.Ticks / f.Ticks;
        }

        public static TimeSpan Multiply(this TimeSpan d, int x)
        {
            return TimeSpan.FromTicks(d.Ticks * x);
        }

        public static ushort ClampToUnsignedShort(this int i)
        {
            if (i <= 0) return 0;
            if (i >= ushort.MaxValue) return ushort.MaxValue;
            return (ushort)i;
        }
    }
}