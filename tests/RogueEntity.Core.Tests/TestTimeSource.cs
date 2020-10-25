using System;
using RogueEntity.Core.Infrastructure.Time;

namespace RogueEntity.Core.Tests
{
    public class TestTimeSource: ITimeSource
    {
        public TimeSpan CurrentTime { get; set; }
        public int FixedStepTime { get; set; }
        public GameTimeState TimeState { get; set; }
    }
}