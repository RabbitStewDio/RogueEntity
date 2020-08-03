using System;
using RogueEntity.Core.Movement.ItemCosts;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Movement.Maps
{
    public sealed class CombinedCostMap : IReadOnlyMapData<MovementCost>
    {
        readonly IReadOnlyMapData<MovementCost>[] entries;

        public CombinedCostMap(params IReadOnlyMapData<MovementCost>[] entries)
        {
            if (entries.Length == 0)
            {
                throw new ArgumentException();
            }

            this.entries = entries;
            this.Height = entries[0].Height;
            this.Width = entries[0].Width;
            for (var index = 1; index < entries.Length; index++)
            {
                var e = entries[index];
                if (e.Height != Height || e.Width != Width)
                {
                    throw new ArgumentException();
                }
            }
        }

        public int Height { get; }
        public int Width { get; }

        public MovementCost this[int x, int y]
        {
            get
            {
                var retval = MovementCost.Blocked;
                foreach (var e in entries)
                {
                    retval = retval.Reduce(e[x, y]);
                }

                return retval;
            }
        }
    }
}