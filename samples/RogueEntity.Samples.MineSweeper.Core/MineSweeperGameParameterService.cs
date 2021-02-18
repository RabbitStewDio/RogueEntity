using RogueEntity.Core.Utils;
using System;

namespace RogueEntity.Samples.MineSweeper.Core
{
    public interface IMineSweeperGameParameterService
    {
        MineSweeperGameParameter WorldParameter { get; }
    }
    
    public class MineSweeperGameParameterService: IMineSweeperGameParameterService
    {
        public MineSweeperGameParameter WorldParameter { get; set; }
    }

    public readonly struct MineSweeperGameParameter : IEquatable<MineSweeperGameParameter>
    {
        public static readonly MineSweeperGameParameter Easy = new MineSweeperGameParameter(new Dimension(10, 10), 10);
        public static readonly MineSweeperGameParameter Normal = new MineSweeperGameParameter(new Dimension(15, 15), 40);
        public static readonly MineSweeperGameParameter Hard = new MineSweeperGameParameter(new Dimension(16, 30), 99);

        public Dimension PlayFieldArea { get; }
        public int MineCount { get; }
        public int Seed { get; }

        public MineSweeperGameParameter(Dimension playFieldArea, int mineCount, int seed = 0)
        {
            PlayFieldArea = playFieldArea;
            MineCount = mineCount;
            Seed = seed;
        }

        public bool Validate()
        {
            return PlayFieldArea.Area > 1 && MineCount > 0 && MineCount < PlayFieldArea.Area;
        }
        
        public MineSweeperGameParameter WithArea(int x, int y)
        {
            return new MineSweeperGameParameter(new Dimension(x, y), MineCount, Seed);
        }
        
        public MineSweeperGameParameter WithMines(int m)
        {
            return new MineSweeperGameParameter(PlayFieldArea, m, Seed);
        }

        public bool Equals(MineSweeperGameParameter other)
        {
            return PlayFieldArea.Equals(other.PlayFieldArea) && MineCount == other.MineCount && Seed == other.Seed;
        }

        /// <summary>
        ///   The player area inside the boundary wall. This area contains acceptable coordinates for the input commands.
        /// </summary>
        public Rectangle ValidInputBounds => new Rectangle(1, 1, PlayFieldArea.Width, PlayFieldArea.Height);

        /// <summary>
        ///   The play area including the boundary wall. This is the rendered area.
        /// </summary>
        public Rectangle PlayFieldBounds => new Rectangle(0, 0, PlayFieldArea.Width + 2, PlayFieldArea.Height + 2); 

        
        public override bool Equals(object obj)
        {
            return obj is MineSweeperGameParameter other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = PlayFieldArea.GetHashCode();
                hashCode = (hashCode * 397) ^ MineCount;
                hashCode = (hashCode * 397) ^ Seed;
                return hashCode;
            }
        }

        public static bool operator ==(MineSweeperGameParameter left, MineSweeperGameParameter right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MineSweeperGameParameter left, MineSweeperGameParameter right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{nameof(PlayFieldArea)}: {PlayFieldArea}, {nameof(MineCount)}: {MineCount}, {nameof(Seed)}: {Seed}";
        }
    }  
}
