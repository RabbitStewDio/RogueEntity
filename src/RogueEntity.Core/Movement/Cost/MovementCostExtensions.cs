using RogueEntity.Api.Time;

namespace RogueEntity.Core.Movement.Cost
{
    public static class MovementCostExtensions
    {
        public static float ToMeterPerSecond<TMovementMode>(this in MovementVelocity<TMovementMode> m,
                                                            ITimeSourceDefinition timeDefinition)
        {
            return (float)(m.Velocity * timeDefinition.UpdateTicksPerSecond);
        }

        public static float ToMeterPerSecond<TMovementMode>(this in MovementPointCost<TMovementMode> m,
                                                            ITimeSourceDefinition timeDefinition)
        {
            return (float) (m.Cost / timeDefinition.UpdateTicksPerSecond);
        }

        public static float ToMeterPerSecond(this in MovementCost m,
                                             ITimeSourceDefinition timeDefinition)
        {
            return (float) (m.Cost / timeDefinition.UpdateTicksPerSecond);
        }
    }
}
