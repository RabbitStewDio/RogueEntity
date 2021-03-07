using System.Collections.Generic;

namespace RogueEntity.Core.Movement
{
    public interface IMovementDataProvider
    {
        IReadOnlyDictionary<IMovementMode, MovementSourceData> MovementCosts { get; }
    }
}
