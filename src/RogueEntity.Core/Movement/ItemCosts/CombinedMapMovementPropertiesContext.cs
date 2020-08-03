using System.Collections.Generic;
using RogueEntity.Core.Infrastructure.Positioning;

namespace RogueEntity.Core.Movement.ItemCosts
{
    public class CombinedMapMovementPropertiesContext : IMapMovementPropertiesContext
    {
        readonly List<IMapMovementPropertiesContext> contexts;

        public CombinedMapMovementPropertiesContext(params IMapMovementPropertiesContext[] c): this((IReadOnlyList<IMapMovementPropertiesContext>)c) {}

        public CombinedMapMovementPropertiesContext(IReadOnlyList<IMapMovementPropertiesContext> c)
        {
            contexts = new List<IMapMovementPropertiesContext>(c);
        }

        public bool TryQueryMovementProperties(Position pos, out MovementCostProperties properties)
        {
            properties = MovementCostProperties.Empty;
            bool haveData = false;
            foreach (var p in contexts)
            {
                if (p.TryQueryMovementProperties(pos, out var props))
                {
                    properties += props;
                    haveData = true;
                }
            }

            return haveData;
        }
    }
}