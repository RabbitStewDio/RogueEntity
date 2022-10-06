using System.Collections.Generic;

namespace RogueEntity.Core.Movement
{
    public interface IMovementDataProvider
    {
        bool TryGet<TMovementMode>(out MovementSourceData m);
        IReadOnlyDictionary<IMovementMode, MovementSourceData> MovementCosts { get; }
    }
}
