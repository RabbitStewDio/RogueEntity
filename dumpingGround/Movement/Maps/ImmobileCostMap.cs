using RogueEntity.Core.Movement.ItemCosts;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Movement.Maps
{
    public class ImmobileCostMap: IReadOnlyMapData<MovementCost>
    {
        public ImmobileCostMap(int width, int height)
        {
            Height = height;
            Width = width;
        }

        public int Height { get; }
        public int Width { get; }

        public MovementCost this[int x, int y] => MovementCost.Blocked;
    }
}