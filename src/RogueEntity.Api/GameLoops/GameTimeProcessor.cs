using System;
using RogueEntity.Api.Time;

namespace RogueEntity.Api.GameLoops
{
    public class GameTimeProcessor
    {
        public TimeSpan TimeStepDuration { get; set; }

        public GameTimeProcessor(float timeStepDuration = 1 / 60f)
        {
            TimeStepDuration = TimeSpan.FromSeconds(timeStepDuration);
            if (TimeStepDuration.Ticks <= 0)
                throw new ArgumentException("Cannot use negative time.");
        }

        public GameTimeProcessor(TimeSpan timeStepDuration)
        {
            if (timeStepDuration.Ticks <= 0)
                throw new ArgumentException("Cannot use negative time.");

            TimeStepDuration = timeStepDuration;
        }

        public int ComputeFixedStepCount(GameTimeState start,
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

        public GameTimeState AdvanceFrameTimeOnly(GameTimeState start, TimeSpan absoluteTime)
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

        public static GameTimeState NextFixedStep(GameTimeState previousState)
        {
            var state = new GameTimeState(previousState.FixedStepCount + 1,
                                          previousState.FixedGameTimeElapsed + previousState.FixedDeltaTime,
                                          previousState.FixedDeltaTime,
                                          previousState.FrameCount, previousState.TotalGameTimeElapsed, previousState.FrameDeltaTime);
            return state;
        }

        public static GameTimeProcessor WithFramesPerSecond(int frames)
        {
            if (frames <= 0)
            {
                throw new ArgumentException();
            }

            return new GameTimeProcessor(1.0f / frames);
        }
    }
}