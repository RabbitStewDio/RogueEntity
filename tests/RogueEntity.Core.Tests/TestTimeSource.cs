using System;
using RogueEntity.Api.Time;

namespace RogueEntity.Core.Tests
{
    public class TestTimeSource: ITimeSource
    {
        public ITimeSourceDefinition TimeSourceDefinition { get; }

        public TestTimeSource(ITimeSourceDefinition timeSourceDefinition)
        {
            TimeSourceDefinition = timeSourceDefinition;
        }

        GameTimeState timeState;
        public TimeSpan CurrentTime { get; set; }
        public int FixedStepTime { get; set; }

        public GameTimeState TimeState
        {
            get => timeState;
            set => timeState = value;
        }

        ref readonly GameTimeState ITimeSource.TimeState => ref timeState;
    }
}