using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;
using System;

namespace RogueEntity.Simple.MineSweeper
{
    public readonly struct MineSweeperPlayerProfile : IEquatable<MineSweeperPlayerProfile>
    {
        public static readonly MineSweeperPlayerProfile Easy = new MineSweeperPlayerProfile(new Dimension(10, 10), 10);
        public static readonly MineSweeperPlayerProfile Normal = new MineSweeperPlayerProfile(new Dimension(20, 20), 25);
        public static readonly MineSweeperPlayerProfile Hard = new MineSweeperPlayerProfile(new Dimension(30, 30), 50);
        
        public Optional<string> Name { get; }
        public Dimension PlayFieldArea { get; }
        public int MineCount { get;  }
        public int Seed { get; }

        public MineSweeperPlayerProfile(Dimension playFieldArea, int mineCount, int seed = default, Optional<string> name = default)
        {
            PlayFieldArea = playFieldArea;
            MineCount = mineCount;
            Seed = seed;
            Name = name;
        }

        public MineSweeperPlayerProfile WithArea(int x, int y)
        {
            return new MineSweeperPlayerProfile(new Dimension(x, y), MineCount, Seed, Name);
        }
        
        public MineSweeperPlayerProfile WithMines(int m)
        {
            return new MineSweeperPlayerProfile(PlayFieldArea, m, Seed, Name);
        }

        public bool Validate()
        {
            return MineCount > 0 && PlayFieldArea.Area > 1;
        }

        public bool Equals(MineSweeperPlayerProfile other)
        {
            return Name.Equals(other.Name) && PlayFieldArea.Equals(other.PlayFieldArea) && MineCount == other.MineCount && Seed == other.Seed;
        }

        public override bool Equals(object obj)
        {
            return obj is MineSweeperPlayerProfile other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name.GetHashCode();
                hashCode = (hashCode * 397) ^ PlayFieldArea.GetHashCode();
                hashCode = (hashCode * 397) ^ MineCount;
                hashCode = (hashCode * 397) ^ Seed;
                return hashCode;
            }
        }

        public static bool operator ==(MineSweeperPlayerProfile left, MineSweeperPlayerProfile right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MineSweeperPlayerProfile left, MineSweeperPlayerProfile right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(PlayFieldArea)}: {PlayFieldArea}, {nameof(MineCount)}: {MineCount}, {nameof(Seed)}: {Seed}";
        }
    }
}
