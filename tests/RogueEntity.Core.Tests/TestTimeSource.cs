using System;
using RogueEntity.Api.Time;

namespace RogueEntity.Core.Tests
{
    public class TestTimeSource: ITimeSource
    {
        public TimeSpan CurrentTime { get; set; }
        public int FixedStepTime { get; set; }
        public GameTimeState TimeState { get; set; }
    }
}