using System;

namespace RogueEntity.SadCons
{
    public readonly struct PlayerProfileContainer<TProfile> : IEquatable<PlayerProfileContainer<TProfile>>
    {
        public readonly Guid Id;
        public readonly TProfile Profile;

        public PlayerProfileContainer(Guid id, TProfile profile)
        {
            Id = id;
            Profile = profile;
        }
        
        public override string ToString()
        {
            return $"{Profile}";
        }

        public bool Equals(PlayerProfileContainer<TProfile> other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerProfileContainer<TProfile> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(PlayerProfileContainer<TProfile> left, PlayerProfileContainer<TProfile> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerProfileContainer<TProfile> left, PlayerProfileContainer<TProfile> right)
        {
            return !left.Equals(right);
        }
    }
}
