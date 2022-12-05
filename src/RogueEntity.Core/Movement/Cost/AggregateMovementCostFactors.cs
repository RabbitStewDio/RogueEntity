using System;
using System.Linq;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;
using System.Collections.Generic;

namespace RogueEntity.Core.Movement.Cost
{
    [EntityComponent]
    [MessagePackObject]
    [DataContract]
    public readonly struct AggregateMovementCostFactors : IEquatable<AggregateMovementCostFactors>
    {
        static readonly EqualityComparer<IMovementMode> movementComparer = EqualityComparer<IMovementMode>.Default;
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly ReadOnlyListWrapper<MovementCost> MovementCosts;

        [SerializationConstructor]
        public AggregateMovementCostFactors(ReadOnlyListWrapper<MovementCost> movementCosts)
        {
            MovementCosts = movementCosts;
        }

        public AggregateMovementCostFactors(params MovementCost[] movementCosts)
        {
            MovementCosts = new ReadOnlyListWrapper<MovementCost>(movementCosts.ToList());
        }

        public bool TryGetMovementCost(IMovementMode mode, out MovementCost cost)
        {
            for (var i = 0; i < MovementCosts.Count; i++)
            {
                var m = MovementCosts[i];
                if (movementComparer.Equals(m.MovementMode, mode))
                {
                    cost = m;
                    return true;
                }
            }

            cost = default;
            return false;
        }
        
        public bool Equals(AggregateMovementCostFactors other)
        {
            return CoreExtensions.EqualsList(MovementCosts, other.MovementCosts);
        }

        public override bool Equals(object obj)
        {
            return obj is AggregateMovementCostFactors other && Equals(other);
        }

        public override int GetHashCode()
        {
            return MovementCosts.GetHashCode();
        }

        public static bool operator ==(AggregateMovementCostFactors left, AggregateMovementCostFactors right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AggregateMovementCostFactors left, AggregateMovementCostFactors right)
        {
            return !left.Equals(right);
        }
    }
}