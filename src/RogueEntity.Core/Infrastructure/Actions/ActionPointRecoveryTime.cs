using EnTTSharp.Annotations;

namespace RogueEntity.Core.Infrastructure.Actions
{
    /// <summary>
    ///   Records the currently active action point recovery frequency and the
    ///   last time recovery has been performed. 
    /// </summary>
    [EntityComponent(EntityConstructor.NonConstructable)]
    public readonly struct ActionPointRecoveryTime
    {
        public readonly int ActionPointsRecovery;
        public readonly int ActionPointsRecoveryFrequency;
        readonly int lastRecoveryTurn;

        public ActionPointRecoveryTime(int actionPointsRecovery,
                                         int actionPointsRecoveryFrequency,
                                         int lastRecoveryTurn)
        {
            this.ActionPointsRecovery = actionPointsRecovery;
            this.ActionPointsRecoveryFrequency = actionPointsRecoveryFrequency;
            this.lastRecoveryTurn = lastRecoveryTurn;
        }

        public ActionPointRecoveryTime ChangeRecovery(int magnitude, int frequency)
        {
            return new ActionPointRecoveryTime(magnitude, frequency, lastRecoveryTurn);
        }

        public ActionPointRecoveryTime Recover(int turn)
        {
            return new ActionPointRecoveryTime(ActionPointsRecovery, ActionPointsRecoveryFrequency, turn);
        }

        public bool IsReady(int turn)
        {
            return this.lastRecoveryTurn + ActionPointsRecoveryFrequency <= turn;
        }

        public override string ToString()
        {
            return $"{nameof(ActionPointsRecovery)}: {ActionPointsRecovery}, {nameof(ActionPointsRecoveryFrequency)}: {ActionPointsRecoveryFrequency}, {nameof(lastRecoveryTurn)}: {lastRecoveryTurn}";
        }
    }
}