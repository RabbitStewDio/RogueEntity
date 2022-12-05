using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;

namespace RogueEntity.Core.Tests.Movement.Pathfinding;

public class PathfinderRegionPainterTest
{
    [Test]
    public void TestEdgeMapping()
    {
        PathfinderRegionPainterJob.DetectEdge(DirectionalityInformation.All)
                               .Should()
                               .Be(DirectionalityInformation.None);

        PathfinderRegionPainterJob.DetectEdge(DirectionalityInformation.UpRight |
                                           DirectionalityInformation.DownLeft)
                               .Should()
                               .Be(DirectionalityInformation.DownLeft | DirectionalityInformation.UpRight);

        PathfinderRegionPainterJob.DetectEdge(DirectionalityInformation.Up |
                                           DirectionalityInformation.UpRight |
                                           DirectionalityInformation.UpLeft |
                                           DirectionalityInformation.Left |
                                           DirectionalityInformation.DownLeft)
                               .Should()
                               .Be(DirectionalityInformation.DownLeft | DirectionalityInformation.UpRight);
    }

    [Test]
    public void TestEdgeMapping2()
    {
        PathfinderRegionPainterJob.TestEdge(DirectionalityInformation.UpRight | DirectionalityInformation.Left | DirectionalityInformation.UpLeft,
                                         DirectionalityInformation.Up,
                                         DirectionalityInformation.UpRight | DirectionalityInformation.UpLeft)
                               .Should()
                               .Be(DirectionalityInformation.UpRight | DirectionalityInformation.UpLeft);

        PathfinderRegionPainterJob.TestEdge(DirectionalityInformation.UpLeft | DirectionalityInformation.Left,
                                         DirectionalityInformation.Up,
                                         DirectionalityInformation.UpRight | DirectionalityInformation.UpLeft)
                               .Should()
                               .Be(DirectionalityInformation.UpLeft);

        PathfinderRegionPainterJob.TestEdge(DirectionalityInformation.All & (~DirectionalityInformation.Down),
                                         DirectionalityInformation.Down,
                                         DirectionalityInformation.DownRight | DirectionalityInformation.DownLeft)
                               .Should()
                               .Be(DirectionalityInformation.DownRight | DirectionalityInformation.DownLeft);

        PathfinderRegionPainterJob.EdgeMappingComplete[(int)(DirectionalityInformation.All & (~DirectionalityInformation.Down))]
                               .Should()
                               .Be(DirectionalityInformation.DownLeft | DirectionalityInformation.DownRight);
    }
}