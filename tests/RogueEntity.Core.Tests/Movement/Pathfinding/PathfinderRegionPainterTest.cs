using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical;

namespace RogueEntity.Core.Tests.Movement.Pathfinding;

public class PathfinderRegionPainterTest
{
    [Test]
    public void TestEdgeMapping()
    {
        
        PathfinderRegionPainter.DetectEdge(DirectionalityInformation.All)
                               .Should()
                               .Be(DirectionalityInformation.None);

        PathfinderRegionPainter.DetectEdge(DirectionalityInformation.UpRight |
                                           DirectionalityInformation.DownLeft)
                               .Should()
                               .Be(DirectionalityInformation.DownLeft | DirectionalityInformation.UpRight);

        PathfinderRegionPainter.DetectEdge(DirectionalityInformation.Up |
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
        PathfinderRegionPainter.TestEdge(DirectionalityInformation.UpRight | DirectionalityInformation.UpLeft| DirectionalityInformation.Left, 
                                         DirectionalityInformation.Up,
                                         DirectionalityInformation.UpRight | DirectionalityInformation.UpLeft)
                               .Should()
                               .Be(DirectionalityInformation.UpRight | DirectionalityInformation.UpLeft);
 
        PathfinderRegionPainter.TestEdge(DirectionalityInformation.UpLeft| DirectionalityInformation.Left, 
                                         DirectionalityInformation.Up,
                                         DirectionalityInformation.UpRight | DirectionalityInformation.UpLeft)
                               .Should()
                               .Be(DirectionalityInformation.UpLeft);
   }
}