using System;
using System.Runtime.Serialization;
using EnTTSharp.Annotations;
using MessagePack;

namespace RogueEntity.Core.Infrastructure.Actions
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [Serializable]
    [DataContract]
    [MessagePackObject]
    public readonly struct MovementPointRecoveryTime
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly int MovementPointsRecovery;
        [DataMember(Order = 1)]
        [Key(1)]
        public readonly int MovementPointsRecoveryFrequency;
        [DataMember(Order = 2)]
        [Key(2)]
        readonly int lastRecoveryTurn;

        [SerializationConstructor]
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