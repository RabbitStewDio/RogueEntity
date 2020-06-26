using EnTTSharp.Annotations;

namespace RogueEntity.Core.Infrastructure.Actions
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    public readonly struct MovementPointRecoveryTime
    {
        public readonly int MovementPointsRecovery;
        public readonly int MovementPointsRecoveryFrequency;
        readonly int lastRecoveryTurn;

        internal MovementPointRecoveryTime(int movementPointsRecovery,
                                           int movementPointsRecoveryFrequency,
                                           int lastRecoveryTurn)
        {
            this.MovementPointsRecovery = movementPointsRecovery;
            this.MovementPointsRecoveryFrequency = movementPointsRecoveryFrequency;
            this.lastRecoveryTurn = lastRecoveryTurn;
        }

        public MovementPointRecoveryTime ChangeRecovery(int magnitude, int frequency)
        {
            return new MovementPointRecoveryTime(magnitude, frequency, lastRecoveryTurn);
        }

        public MovementPointRecoveryTime Recover(int turn)
        {
            return new MovementPointRecoveryTime(MovementPointsRecovery, MovementPointsRecoveryFrequency, turn);
        }

        public bool IsReady(int turn)
        {
            return this.lastRecoveryTurn + MovementPointsRecoveryFrequency <= turn;
        }

        public override string ToString()
        {
            return $"{nameof(MovementPointsRecovery)}: {MovementPointsRecovery}, {nameof(MovementPointsRecoveryFrequency)}: {MovementPointsRecoveryFrequency}, {nameof(lastRecoveryTurn)}: {lastRecoveryTurn}";
        }
    }
}