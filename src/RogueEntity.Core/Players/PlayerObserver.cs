using RogueEntity.Core.Positioning;
using System;

namespace RogueEntity.Core.Players
{
    /// <summary>
    ///   Records the position of a player's camera focus. Players that are inactive have no
    ///   observer, while players can have more than one observer if needed (for instance
    ///   while standing in a room with a crystal ball). 
    /// </summary>
    public readonly struct PlayerObserver : IEquatable<PlayerObserver>
    {
        public readonly Guid Id;
        public readonly bool Primary;
        public readonly PlayerTag Player;
        public readonly Position Position;

        public PlayerObserver(Guid id, PlayerTag player, bool primary, Position position)
        {
            Primary = primary;
            Id = id;
            Player = player;
            Position = position;
        }

        public bool Equals(PlayerObserver other)
        {
            return Id.Equals(other.Id) && Primary == other.Primary && Player.Equals(other.Player) && Position.Equals(other.Position);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerObserver other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Primary.GetHashCode();
                hashCode = (hashCode * 397) ^ Player.GetHashCode();
                hashCode = (hashCode * 397) ^ Position.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(PlayerObserver left, PlayerObserver right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerObserver left, PlayerObserver right)
        {
            return !left.Equals(right);
        }
    }
}
