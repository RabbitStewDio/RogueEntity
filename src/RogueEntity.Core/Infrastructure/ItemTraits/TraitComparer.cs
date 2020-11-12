using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Infrastructure.ItemTraits
{
    public class TraitComparer : IComparer<ITrait>
    {
        public static readonly TraitComparer Default = new TraitComparer();

        public int Compare(ITrait x, ITrait y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            var p = x.Priority.CompareTo(y.Priority);
            if (p != 0)
            {
                return p;
            }

            return string.Compare(x.Id.Id, y.Id.Id, StringComparison.Ordinal);
        }
    }
}