using System;
using FluentAssertions;
using Microsoft.Extensions.ObjectPool;
using NUnit.Framework;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.MovementPlaning.GoalFinding.SingleLevel;
using RogueEntity.Core.MovementPlaning.Goals;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.SpatialQueries;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System.Diagnostics.CodeAnalysis;
using static RogueEntity.Core.Tests.Movement.PathfindingTestUtil;
using Assert = RogueEntity.Core.Utils.Assert;

namespace RogueEntity.Core.Tests.Movement.GoalFinding
{
    [TestFixture]
    [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
    public class GoalFindingTest
    {
        ItemContextBackend<ItemReference> context;
        GoalRegistry goalRegistry;
        SpatialQueryRegistry spatialQueryRegistry;
        GridItemPlacementService<ItemReference> itemPlacementService;

        const string EmptyRoom = @"
 // 9x9; an empty room
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
";

        const string EmptyRoomResult = @"
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
 ### ,  @  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,   1 ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,   2 ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,   3 ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,   4 ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,   5 ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,   6 , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
";

        const string DiagonalBlockRoom = @"
 // 9x9; an empty room
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  , ### ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  , ### ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
";

        const string DiagonalRoomResult = @"
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
 ### ,  @  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  1  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  2  ,   . , ### ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  3  , ### ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  4  ,  5  ,   6 ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,   7 ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,   8 , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
";

        [SetUp]
        public void SetUp()
        {
            var gridMapContext = new DefaultGridPositionContextBackend<ItemReference>();
            gridMapContext.WithDefaultMapLayer(TestMapLayers.One, DynamicDataViewConfiguration.Default16X16);


            context = new ItemContextBackend<ItemReference>(new ItemReferenceMetaData());
            context.EntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<ItemReference>>();
            context.EntityRegistry.RegisterNonConstructable<EntityGridPosition>();
            context.EntityRegistry.RegisterNonConstructable<EntityGridPositionChangedMarker>();
            context.EntityRegistry.RegisterNonConstructable<GoalMarker<TestGoal>>();
            context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>("Goal")
                                          .WithTrait(new GoalMarkerTrait<ItemReference, TestGoal>(10))
                                          .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(TestMapLayers.One))
            );

            goalRegistry = new GoalRegistry();
            goalRegistry.RegisterGoalEntity<ItemReference, TestGoal>();

            spatialQueryRegistry = new SpatialQueryRegistry();
            spatialQueryRegistry.Register(new BruteForceSpatialQueryBackend<ItemReference>(context.EntityRegistry));

            itemPlacementService = new GridItemPlacementService<ItemReference>(context.ItemResolver, gridMapContext);
        }


        public readonly struct PathFinderTestParameters
        {
            public readonly string SourceText;
            public readonly string ResultText;
            public readonly Position2D Origin;
            public readonly (Position2D pos, float strength)[] Targets;

            public PathFinderTestParameters(string sourceText, string resultText, Position2D origin, params (Position2D, float)[] targets)
            {
                this.SourceText = sourceText;
                this.ResultText = resultText;
                this.Origin = origin;
                this.Targets = targets;
            }
        }

        static readonly TestCaseData[] TestData =
        {
            new TestCaseData(new PathFinderTestParameters(EmptyRoom, EmptyRoomResult, new Position2D(1, 1), (new Position2D(7, 7), 10)))
                .SetName(nameof(EmptyRoom)),
            new TestCaseData(new PathFinderTestParameters(DiagonalBlockRoom, DiagonalRoomResult, new Position2D(1, 1), (new Position2D(7, 7), 10)))
                .SetName(nameof(DiagonalBlockRoom)),
            new TestCaseData(new PathFinderTestParameters(EmptyRoom, EmptyRoomResult, new Position2D(1, 1), (new Position2D(7, 7), 20), (new Position2D(7, 1), 10)))
                .SetName(nameof(EmptyRoom) + " with 2 goals"),
        };


        [Test]
        [TestCaseSource(nameof(TestData))]
        public void ValidatePathFinding(PathFinderTestParameters p)
        {
            var resistanceMap = ParseMap(p.SourceText, out var bounds);
            Console.WriteLine("Using room layout \n" + TestHelpers.PrintMap(resistanceMap, bounds));

            foreach (var (t, s) in p.Targets)
            {
                InstantiateGoalTarget(t.X, t.Y, s);
            }

            var startPosition = EntityGridPosition.Of(TestMapLayers.One, p.Origin.X, p.Origin.Y);

            var pfs = CreateGoalFinderSource(resistanceMap, bounds);
            var pf = pfs.GetGoalFinder()
                        .WithGoal<TestGoal>()
                        .Build(new AggregateMovementCostFactors(new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1)));

            pf.TryFindPath(startPosition, out var result).Should().BeTrue();
            result.resultHint.Should().Be(PathFinderResult.Found);
            result.path.Should().NotBeEmpty();

            var expectedResultMap = ParseResultMap(p.ResultText, out _);
            var producedResultMap = CreateResult(resistanceMap, result.path, result.path.Origin, bounds);
            Console.WriteLine("Found path " + string.Join(",", result.path));
            Console.WriteLine("Expected Result\n" + PrintResultMap(expectedResultMap, bounds));
            Console.WriteLine("Computed Result\n" + PrintResultMap(producedResultMap, bounds));

            TestHelpers.AssertEquals(producedResultMap, expectedResultMap, bounds, default, PrintResultMap);
        }

        void InstantiateGoalTarget(int tx, int ty, float strength)
        {
            var targetPosition = EntityGridPosition.Of(TestMapLayers.One, tx, ty);
            var target = context.ItemResolver.Instantiate("Goal");
            context.ItemResolver.TryUpdateData(target, new GoalMarker<TestGoal>(strength), out _).Should().BeTrue();

            itemPlacementService.TryPlaceItem(target, Position.From(targetPosition)).Should().BeTrue();
        }

        SingleLevelGoalFinderSource CreateGoalFinderSource(DynamicDataView2D<float> resistanceMap, Rectangle bounds)
        {
            var outboundDirectionalityMapSystem = new OutboundMovementDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
            outboundDirectionalityMapSystem.MarkGloballyDirty();
            outboundDirectionalityMapSystem.Process();
            outboundDirectionalityMapSystem.ResultView.TryGetView(0, out var outboundDirectionalityMap).Should().BeTrue();

            Assert.NotNull(outboundDirectionalityMap);
            
            var inboundDirectionalityMapSystem = new InboundMovementDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
            inboundDirectionalityMapSystem.MarkGloballyDirty();
            inboundDirectionalityMapSystem.Process();
            inboundDirectionalityMapSystem.ResultView.TryGetView(0, out var inboundDirectionalityMap).Should().BeTrue();

            Assert.NotNull(inboundDirectionalityMap);
            
            Console.WriteLine("Directionality: \n" + outboundDirectionalityMap.PrintEdges(bounds));

            var ms = new MovementDataCollector();
            ms.RegisterMovementSource<WalkingMovement>(WalkingMovement.Instance, resistanceMap.As3DMap(0), inboundDirectionalityMap.As3DMap(0), outboundDirectionalityMap.As3DMap(0));

            var policy = new SingleLevelGoalFinderPolicy(new SingleLevelPathPool());
            var pfs = new SingleLevelGoalFinderSource(policy, goalRegistry, spatialQueryRegistry, ms);
            return pfs;
        }
    }

    public readonly struct TestGoal : IGoal
    { }
}
