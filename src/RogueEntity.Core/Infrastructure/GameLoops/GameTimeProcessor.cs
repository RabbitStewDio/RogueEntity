using RogueEntity.Api.Time;
using System;

namespace RogueEntity.Core.Infrastructure.GameLoops
{
    public class GameTimeProcessor
    {
        public TimeSpan TimeStepDuration { get; }
        public double TicksPerSecond { get; }

        public GameTimeProcessor(double timeStepDuration = 1 / 60f)
        {
            TimeStepDuration = TimeSpan.FromSeconds(timeStepDuration);
            if (TimeStepDuration.Ticks <= 0)
                throw new ArgumentException("Cannot use negative time.");
            TicksPerSecond = 1000 / TimeStepDuration.TotalMilliseconds;
        }

        public GameTimeProcessor(TimeSpan timeStepDuration)
        {
            if (timeStepDuration.Ticks <= 0)
                throw new ArgumentException("Cannot use negative time.");

            TimeStepDuration = timeStepDuration;
        }

        public int ComputeFixedStepCount(in GameTimeState start,
                                         TimeSpan absoluteTime,
                                         ref TimeSpan fixedUpdateHandledTime,
                                         ref TimeSpan fixedUpdateTargetTime)
        {
            var timePassedThisFrame = absoluteTime - start.TotalGameTimeElapsed;
            fixedUpdateTargetTime += timePassedThisFrame;

            var fc = 0;
            while (fixedUpdateHandledTime + TimeStepDuration <= fixedUpdateTargetTime)
            {
                fc += 1;
                fixedUpdateHandledTime += TimeStepDuration;
            }

            return fc;
        }

        public GameTimeState AdvanceFrameTimeOnly(in GameTimeState start, TimeSpan absoluteTime)
        {
            var endTime = absoluteTime;
            var currentFixed = start.FixedGameTimeElapsed;
            var fc = start.FixedStepCount;
            var endState = new GameTimeState(fc,
                                             currentFixed, start.FixedDeltaTime,
                                             start.FrameCount + 1, endTime,
                                             absoluteTime - start.TotalGameTimeElapsed);
            return endState;
        }

        public static GameTimeState NextFixedStep(in GameTimeState previousState)
        {
            var state = new GameTimeState(previousState.FixedStepCount + 1,
                                          previousState.FixedGameTimeElapsed + previousState.FixedDeltaTime,
                                          previousState.FixedDeltaTime,
                                          previousState.FrameCount, previousState.TotalGameTimeElapsed, previousState.FrameDeltaTime);
            return state;
        }

        public static GameTimeProcessor WithFramesPerSecond(double frames)
        {
            if (frames <= 0)
            {
                throw new ArgumentException();
            }

            return new GameTimeProcessor(1.0f / frames);
        }
    }
}