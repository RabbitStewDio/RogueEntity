using EnTTSharp.Entities.Attributes;
using JetBrains.Annotations;
using MessagePack;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Grid;
using System;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Movement.GridMovement
{
    [EntityComponent]
    [MessagePackObject]
    [DataContract]
    public readonly struct GridMoveCommand
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly EntityGridPosition MoveTo;

        [DataMember(Order = 1)]
        [Key(1)]
        public readonly Optional<TimeSpan> FinishTime;

        [DataMember(Order = 2)]
        [Key(2)]
        public readonly Optional<IMovementMode> MovementMode;

        [SerializationConstructor]
        public GridMoveCommand(EntityGridPosition moveTo, Optional<IMovementMode> movementMode = default, Optional<TimeSpan> finishTurn = default)
        {
            MoveTo = moveTo;
            MovementMode = movementMode;
            FinishTime = finishTurn;
        }

        public GridMoveCommand WithFinishTime([NotNull] IMovementMode mode, TimeSpan targetTurn)
        {
            if (mode == null)
            {
                throw new ArgumentNullException(nameof(mode));
            }

            return new GridMoveCommand(MoveTo, Optional.ValueOf(mode), targetTurn);
        }

        public override string ToString()
        {
            return $"GridMoveCommand({nameof(MoveTo)}: {MoveTo}, {nameof(FinishTime)}: {FinishTime}, {nameof(MovementMode)}: {MovementMode})";
        }
    }
}
