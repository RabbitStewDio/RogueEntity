using System;

namespace RogueEntity.Api.Time
{
    public readonly struct GameTimeState : IEquatable<GameTimeState>
    {
        public readonly int FrameCount;
        public readonly int FixedStepCount;
        public readonly TimeSpan TotalGameTimeElapsed;
        public readonly TimeSpan FrameDeltaTime;
        public readonly TimeSpan FixedGameTimeElapsed;
        public readonly TimeSpan FixedDeltaTime;

        public GameTimeState(TimeSpan fixedDeltaTime) : this()
        {
            FixedDeltaTime = fixedDeltaTime;
        }

        public GameTimeState(int fixedStepCount, 
                             TimeSpan fixedGameTimeElapsed, 
                             TimeSpan fixedDeltaTime,
                             int frameCount, 
                             TimeSpan totalGameTimeElapsed,
                             TimeSpan frameDeltaTime)
        {
            FixedStepCount = fixedStepCount;
            FrameCount = frameCount;
            TotalGameTimeElapsed = totalGameTimeElapsed;
            FrameDeltaTime = frameDeltaTime;
            FixedGameTimeElapsed = fixedGameTimeElapsed;
            FixedDeltaTime = fixedDeltaTime;
        }

        public override string ToString()
        {
            return $"{nameof(FrameCount)}: {FrameCount}, {nameof(FixedStepCount)}: {FixedStepCount}, {nameof(TotalGameTimeElapsed)}: {TotalGameTimeElapsed}, {nameof(FixedGameTimeElapsed)}: {FixedGameTimeElapsed}";
        }

        public bool Equals(GameTimeState other)
        {
            return FrameCount == other.FrameCount
                   && FixedStepCount == other.FixedStepCount
                   && TotalGameTimeElapsed.Equals(other.TotalGameTimeElapsed)
                   && FixedGameTimeElapsed.Equals(other.FixedGameTimeElapsed);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is GameTimeState state && Equals(state);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FrameCount;
                hashCode = (hashCode * 397) ^ FixedStepCount;
                hashCode = (hashCode * 397) ^ TotalGameTimeElapsed.GetHashCode();
                hashCode = (hashCode * 397) ^ FixedGameTimeElapsed.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(GameTimeState left, GameTimeState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameTimeState left, GameTimeState right)
        {
            return !left.Equals(right);
        }
    }
}