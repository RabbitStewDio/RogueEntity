using System;
using System.Linq;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Movement.Cost
{
    [EntityComponent]
    [MessagePackObject]
    [DataContract]
    public readonly struct PathfindingMovementCostFactors : IEquatable<PathfindingMovementCostFactors>
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly ReadOnlyListWrapper<MovementCost> MovementCosts;

        [SerializationConstructor]
        public PathfindingMovementCostFactors(ReadOnlyListWrapper<MovementCost> movementCosts)
        {
            MovementCosts = movementCosts;
        }

        public PathfindingMovementCostFactors(params MovementCost[] movementCosts)
        {
            MovementCosts = new ReadOnlyListWrapper<MovementCost>(movementCosts.ToList());
        }

        public bool Equals(PathfindingMovementCostFactors other)
        {
            return CoreExtensions.EqualsList(MovementCosts, other.MovementCosts);
        }

        public override bool Equals(object obj)
        {
            return obj is PathfindingMovementCostFactors other && Equals(other);
        }

        public override int GetHashCode()
        {
            return MovementCosts.GetHashCode();
        }

        public static bool operator ==(PathfindingMovementCostFactors left, PathfindingMovementCostFactors right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PathfindingMovementCostFactors left, PathfindingMovementCostFactors right)
        {
            return !left.Equals(right);
        }
    }
}