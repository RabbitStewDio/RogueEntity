using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Tests.Positioning;

public class DirectionTest
{
    [Test]
    public void MoveClockwise()
    {
        Direction.Up.MoveClockwise(0).Should().Be(Direction.Up);
        Direction.Up.MoveClockwise(1).Should().Be(Direction.UpRight);
        Direction.Up.MoveClockwise(8).Should().Be(Direction.Up);
        Direction.Up.MoveClockwise(-8).Should().Be(Direction.Up);
    }
    
    [Test]
    public void MoveCounterClockwise()
    {
        Direction.Up.MoveCounterClockwise(0).Should().Be(Direction.Up);
        Direction.Up.MoveCounterClockwise(1).Should().Be(Direction.UpLeft);
    }
}