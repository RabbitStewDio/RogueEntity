using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests.Fixtures;
using System;

namespace RogueEntity.Core.Tests.Positioning.Grid
{
    public class EntityGridPositionTest
    {
        [Test]
        public void DefaultIsInvalid()
        {
            var p = new EntityGridPosition();
            p.IsInvalid.Should().BeTrue();
        }

        [Test]
        public void OutOfRangeX()
        {
            this.Invoking(_ => EntityGridPosition.Of(TestMapLayers.One, int.MaxValue, 0)).Should().Throw<ArgumentOutOfRangeException>();
            this.Invoking(_ => EntityGridPosition.Of(TestMapLayers.One, 0, int.MaxValue)).Should().Throw<ArgumentOutOfRangeException>();
            this.Invoking(_ => EntityGridPosition.Of(TestMapLayers.One, 0, 0, int.MaxValue)).Should().Throw<ArgumentOutOfRangeException>();
            this.Invoking(_ => EntityGridPosition.Of(TestMapLayers.One, int.MinValue, 0)).Should().Throw<ArgumentOutOfRangeException>();
            this.Invoking(_ => EntityGridPosition.Of(TestMapLayers.One, 0, int.MinValue)).Should().Throw<ArgumentOutOfRangeException>();
            this.Invoking(_ => EntityGridPosition.Of(TestMapLayers.One, 0, 0, int.MinValue)).Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void MaxValueTest()
        {
            var p = EntityGridPosition.Of(TestMapLayers.One, EntityGridPosition.MaxXYValue, EntityGridPosition.MaxXYValue, EntityGridPosition.MaxZValue);
            p.IsInvalid.Should().BeFalse();
            p.X.Should().Be(EntityGridPosition.MaxXYValue);
            p.Y.Should().Be(EntityGridPosition.MaxXYValue);
            p.Z.Should().Be(EntityGridPosition.MaxZValue);
        }
        
        [Test]
        public void MinValueTest()
        {
            var p = EntityGridPosition.Of(TestMapLayers.One, EntityGridPosition.MinXYValue, EntityGridPosition.MinXYValue, EntityGridPosition.MinZValue);
            p.IsInvalid.Should().BeFalse();
            p.X.Should().Be(EntityGridPosition.MinXYValue);
            p.Y.Should().Be(EntityGridPosition.MinXYValue);
            p.Z.Should().Be(EntityGridPosition.MinZValue);
        }
    }
}