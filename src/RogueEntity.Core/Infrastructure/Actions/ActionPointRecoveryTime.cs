using System;
using System.Runtime.Serialization;
using EnTTSharp.Annotations;
using MessagePack;

namespace RogueEntity.Core.Infrastructure.Actions
{
    /// <summary>
    ///   Records the currently active action point recovery frequency and the
    ///   last time recovery has been performed. 
    /// </summary>
    [EntityComponent(EntityConstructor.NonConstructable)]
    [Serializable]
    [DataContract]
    [MessagePackObject]
    public readonly struct ActionPointRecoveryTime
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly int ActionPointsRecovery;
        [DataMember(Order = 1)]
        [Key(1)]
        public readonly int ActionPointsRecoveryFrequency;
        [DataMember(Order = 2)]
        [Key(2)]
        readonly int lastRecoveryTurn;

        [SerializationConstructor]
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