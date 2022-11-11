using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Movement.Cost;

public interface IMovementStyleInformationTrait: IItemComponentDesignTimeInformationTrait<(IMovementMode, DistanceCalculation)>
{
}