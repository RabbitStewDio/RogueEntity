using System;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.GoalFinding.SingleLevel;
using RogueEntity.Core.Movement.Goals;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.SpatialQueries;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using static RogueEntity.Core.Tests.Movement.PathfindingTestUtil;

namespace RogueEntity.Core.Tests.Movement.GoalFinding
{
    [TestFixture]
    public class GoalFindingTest
    {
        ItemContextBackend<object, ItemReference> context;
        GoalRegistry goalRegistry;
        SpatialQueryRegistry spatialQueryRegistry;

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
 ### ,  .  ,   6 ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,   5 ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,   4 ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,   3 ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,   2 ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,   1 , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
";

        [SetUp]
        public void SetUp()
        {
            var gridMapContext = new DefaultGridPositionContextBackend<ItemReference>();
            gridMapContext.WithDefaultMapLayer(TestMapLayers.One, DynamicDataViewConfiguration.Default_16x16);


            context = new ItemContextBackend<object, ItemReference>(new ItemReferenceMetaData());
            context.EntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<object, ItemReference>>();
            context.EntityRegistry.RegisterNonConstructable<EntityGridPosition>();
            context.EntityRegistry.RegisterNonConstructable<EntityGridPositionChangedMarker>();
            context.EntityRegistry.RegisterNonConstructable<GoalMarker<TestGoal>>();
            context.ItemRegistry.Register(new ReferenceItemDeclaration<object, ItemReference>("Goal")
                                          .WithTrait(new GoalMarkerTrait<object, ItemReference, TestGoal>(10))
                                          .WithTrait(new ReferenceItemGridPositionTrait<object, ItemReference>(context.ItemResolver, gridMapContext, TestMapLayers.One))
            );

            goalRegistry = new GoalRegistry();
            goalRegistry.RegisterGoalEntity<ItemReference, TestGoal>();

            spatialQueryRegistry = new SpatialQueryRegistry();
            spatialQueryRegistry.Register(new BruteForceSpatialQueryBackend<ItemReference>(context.EntityRegistry));
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
            new TestCaseData(new PathFinderTestParameters(EmptyRoom, EmptyRoomResult, new Position2D(1, 1), (new Position2D(7, 7), 20), (new Position2D(7, 1), 10)))
                .SetName(nameof(EmptyRoom) + " with 2 goals"),
        };

        [Test]
        [TestCaseSource(nameof(TestData))]
        public void ValidatePathFinding(PathFinderTestParameters p)
        {
            var gameContext = new object();

            var resistanceMap = ParseMap(p.SourceText, out var bounds);
            Console.WriteLine("Using room layout \n" + TestHelpers.PrintMap(resistanceMap, bounds));

            foreach (var (t, s) in p.Targets)
            {
                InstantiateGoalTarget(t.X, t.Y, s, gameContext);
            }

            var startPosition = EntityGridPosition.Of(TestMapLayers.One, p.Origin.X, p.Origin.Y);

            var pfs = CreateGoalFinderSource(resistanceMap, bounds);
            var pf = pfs.GetGoalFinder()
                        .WithGoal<TestGoal>()
                        .Build(new PathfindingMovementCostFactors(new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1)));

            var result = pf.TryFindPath(startPosition, out var resultPath);
            result.Should().Be(PathFinderResult.Found);
            resultPath.Should().NotBeEmpty();

            var expectedResultMap = ParseResultMap(p.ResultText, out _);
            var producedResultMap = CreateResult(resistanceMap, resultPath, startPosition, bounds);
            Console.WriteLine("Expected Result\n" + PrintResultMap(expectedResultMap, bounds));
            Console.WriteLine("Computed Result\n" + PrintResultMap(producedResultMap, bounds));

            TestHelpers.AssertEquals(producedResultMap, expectedResultMap, bounds, default, PrintResultMap);
        }

        void InstantiateGoalTarget(int tx, int ty, float strength, object gameContext)
        {
            var targetPosition = EntityGridPosition.Of(TestMapLayers.One, tx, ty);
            var target = context.ItemResolver.Instantiate(gameContext, "Goal");
            context.ItemResolver.TryUpdateData(target, gameContext, new GoalMarker<TestGoal>(strength), out _).Should().BeTrue();
            context.ItemResolver.TryUpdateData(target, gameContext, targetPosition, out _).Should().BeTrue();
        }

        SingleLevelGoalFinderSource CreateGoalFinderSource(DynamicDataView2D<float> resistanceMap, Rectangle bounds)
        {
            var directionalityMapSystem = new MovementResistanceDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
            directionalityMapSystem.MarkGloballyDirty();
            directionalityMapSystem.Process();
            directionalityMapSystem.ResultView.TryGetView(0, out var directionalityMap).Should().BeTrue();

            Console.WriteLine("Directionality: \n" + TestHelpers.PrintMap(directionalityMap.Transform(e => $"[{e.ToFormattedString()}] "), bounds));
            
            var pfs = new SingleLevelGoalFinderSource(new SingleLevelGoalFinderPolicy(), goalRegistry, spatialQueryRegistry);
            pfs.RegisterMovementSource(WalkingMovement.Instance, resistanceMap.As3DMap(0), directionalityMap.As3DMap(0));
            return pfs;
        }
    }

    public readonly struct TestGoal : IGoal
    { }
}
