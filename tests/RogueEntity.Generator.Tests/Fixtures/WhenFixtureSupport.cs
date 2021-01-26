using FluentAssertions;
using System;

namespace RogueEntity.Generator.Tests.Fixtures
{
    public abstract class WhenFixtureSupport
    {
        public bool LastResult { get; private set; }
        
        public bool When(Func<bool> action)
        {
            LastResult = action();
            return LastResult;
        }

        public bool When(Func<object, bool> action)
        {
            LastResult = action(null);
            return LastResult;
        }

        protected void Then_Operation_Should_Succeed()
        {
            LastResult.Should().BeTrue();
        }

        protected void Then_Operation_Should_Fail()
        {
            LastResult.Should().BeFalse();
        }


    }
}
