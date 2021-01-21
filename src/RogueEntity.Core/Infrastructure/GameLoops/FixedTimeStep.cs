using System;

namespace RogueEntity.Core.Infrastructure.GameLoops
{
    public readonly struct FixedTimeStep : IEquatable<FixedTimeStep>
    {
        public readonly int FrameCount;
        public readonly TimeSpan ElapsedTime;
        public readonly TimeSpan TotalTime;

        public FixedTimeStep(int frameCount, TimeSpan elapsedTime, TimeSpan totalTime)
        {
            FrameCount = frameCount;
            ElapsedTime = elapsedTime;
            TotalTime = totalTime;
        }

        public override string ToString()
        {
            return
                $"{nameof(FrameCount)}: {FrameCount}, {nameof(ElapsedTime)}: {ElapsedTime}, {nameof(TotalTime)}: {TotalTime}";
        }

        public bool Equals(FixedTimeStep other)
        {
            return FrameCount == other.FrameCount
                   && ElapsedTime.Equals(other.ElapsedTime)
                   && TotalTime.Equals(other.TotalTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is FixedTimeStep && Equals((FixedTimeStep) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FrameCount;
                hashCode = (hashCode * 397) ^ ElapsedTime.GetHashCode();
                hashCode = (hashCode * 397) ^ TotalTime.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(FixedTimeStep left, FixedTimeStep right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FixedTimeStep left, FixedTimeStep right)
        {
            return !left.Equals(right);
        }
    }
}