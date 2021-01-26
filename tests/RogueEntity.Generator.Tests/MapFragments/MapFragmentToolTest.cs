using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Generator.MapFragments;
using RogueEntity.Generator.Tests.Fixtures;

namespace RogueEntity.Generator.Tests.MapFragments
{
    public class MapFragmentToolTest: MapOperationsFixtureBase
    {
        MapFragmentTool PlacementTool => MapBuilder.ForFragmentPlacement(new FixedRandomGeneratorSource());
        
        MapFragment Given_A_MapFragment_From_File(string file)
        {
            var mfp = new MapFragmentParser();
            mfp.TryParseFromFile(file, out var mf).Should().BeTrue();
            return mf;
        }

        [Test]
        public void MapFragmentPlacement()
        {
            var mf = Given_A_MapFragment_From_File(@"MapFragments/placement-test.mapfragment");

            PlacementTool.CopyToMap(mf, EntityGridPosition.Of(MapLayer.Indeterminate, 0, 0));

            Items.Then_Position(Position.Of(FloorLayer, 0, 0)).Should().ContainEntityOfType(BulkFloor1);
            Items.Then_Position(Position.Of(FloorLayer, 1, 0)).Should().BeEmpty();
            Items.Then_Position(Position.Of(FloorLayer, 2, 0)).Should().ContainEntityOfType(BulkFloor2);
            Items.Then_Position(Position.Of(FloorLayer, 0, 1)).Should().ContainEntityOfType(BulkFloor2);
            Items.Then_Position(Position.Of(FloorLayer, 1, 1)).Should().ContainEntityOfType(BulkFloor1);
            Items.Then_Position(Position.Of(FloorLayer, 2, 1)).Should().ContainEntityOfType(BulkFloor1);
            Items.Then_Position(Position.Of(FloorLayer, 0, 2)).Should().ContainEntityOfType(BulkFloor1);
            Items.Then_Position(Position.Of(FloorLayer, 1, 2)).Should().ContainEntityOfType(BulkFloor3);
            Items.Then_Position(Position.Of(FloorLayer, 2, 2)).Should().ContainEntityOfType(BulkFloor2);
            
            Items.Then_Position(Position.Of(ItemLayer, 0, 0)).Should().BeEmpty();
            Items.Then_Position(Position.Of(ItemLayer, 1, 0)).Should().BeEmpty();
            Items.Then_Position(Position.Of(ItemLayer, 2, 0)).Should().ContainEntityOfType(BulkItem1);
            Items.Then_Position(Position.Of(ItemLayer, 0, 1)).Should().ContainEntityOfType(BulkItem1);
            Items.Then_Position(Position.Of(ItemLayer, 1, 1)).Should().BeEmpty();
            Items.Then_Position(Position.Of(ItemLayer, 2, 1)).Should().BeEmpty();
            Items.Then_Position(Position.Of(ItemLayer, 0, 2)).Should().BeEmpty();
            Items.Then_Position(Position.Of(ItemLayer, 1, 2)).Should().ContainEntityOfType(ReferenceItem1);
            Items.Then_Position(Position.Of(ItemLayer, 2, 2)).Should().ContainEntityOfType(BulkItem1);
            
            Actors.Then_Position(Position.Of(ActorLayer, 0, 0)).Should().BeEmpty();
            Actors.Then_Position(Position.Of(ActorLayer, 1, 0)).Should().BeEmpty();
            Actors.Then_Position(Position.Of(ActorLayer, 2, 0)).Should().BeEmpty();
            Actors.Then_Position(Position.Of(ActorLayer, 0, 1)).Should().BeEmpty();
            Actors.Then_Position(Position.Of(ActorLayer, 1, 1)).Should().ContainEntityOfType(Actor);
            Actors.Then_Position(Position.Of(ActorLayer, 2, 1)).Should().BeEmpty();
            Actors.Then_Position(Position.Of(ActorLayer, 0, 2)).Should().BeEmpty();
            Actors.Then_Position(Position.Of(ActorLayer, 1, 2)).Should().BeEmpty();
            Actors.Then_Position(Position.Of(ActorLayer, 2, 2)).Should().BeEmpty();
            
        }
    }
}
