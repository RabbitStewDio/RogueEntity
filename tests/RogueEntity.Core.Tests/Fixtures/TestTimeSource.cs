using RogueEntity.Api.Time;
using System;

namespace RogueEntity.Core.Tests.Fixtures
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