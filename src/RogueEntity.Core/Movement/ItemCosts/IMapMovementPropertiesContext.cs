using RogueEntity.Core.Infrastructure.Positioning;

namespace RogueEntity.Core.Movement.ItemCosts
{
    public interface IMapMovementPropertiesContext
    {
        bool TryQueryMovementProperties(Position pos, out MovementCostProperties properties);
    }
}