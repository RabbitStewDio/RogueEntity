using RogueEntity.Core.Movement.ItemCosts;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Movement.Maps
{
    public class FlyingCostMap : IReadOnlyMapData<MovementCost>
    {
        readonly IReadOnlyMapData<MovementCostProperties> flagData;
        readonly float baseCost;

        public FlyingCostMap(IReadOnlyMapData<MovementCostProperties> flagData,
                             float baseCost)
        {
            this.flagData = flagData;
            this.baseCost = baseCost;
        }

        public int Height => flagData.Width;
        public int Width => flagData.Height;


        public MovementCost this[int x, int y]
        {
            get
            {
                var mc = flagData[x, y].Flying;
                return mc.Apply(baseCost);
            }
        }
    }
}