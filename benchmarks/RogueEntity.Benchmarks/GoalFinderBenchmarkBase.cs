using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Profiler.Api;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.MovementPlaning.GoalFinding.SingleLevel;
using RogueEntity.Core.MovementPlaning.Goals;
using RogueEntity.Core.MovementPlaning.Pathfinding;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Positioning.SpatialQueries;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Benchmarks
{
    public abstract class GoalFinderBenchmarkBase
    {
        readonly MapLayer layer = new MapLayer(1, "Default");

        readonly ItemDeclarationId goalEntitiyId = "goal-entity";

        readonly ItemContextBackend<ItemReference> entities;
        readonly string id;
        readonly List<EntityGridPosition> positions;
        readonly SingleLevelGoalFinderSource pathfinderSource;
        readonly GoalRegistry goalRegistry;
        readonly SpatialQueryRegistry queryRegistry;
        readonly BufferList<(EntityGridPosition, IMovementMode)> pathBuffer;
        readonly MovementDataCollector movementDataCollector;

        Rectangle bounds;

        protected GoalFinderBenchmarkBase(string id)
        {
            this.id = id;

            var mapContext = new DefaultGridPositionContextBackend<ItemReference>();
            mapContext.WithDefaultMapLayer(layer);

            entities = new ItemContextBackend<ItemReference>(new ItemReferenceMetaData());
            entities.EntityRegistry.RegisterNonConstructable<EntityGridPosition>();
            entities.EntityRegistry.RegisterNonConstructable<EntityGridPositionChangedMarker>();
            entities.EntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<ItemReference>>();
            entities.EntityRegistry.RegisterNonConstructable<GoalMarker<PerformanceGoal>>();
            entities.ItemRegistry.Register(new ReferenceItemDeclaration< ItemReference>(goalEntitiyId)
                                           .WithTrait(new ReferenceItemGridPositionTrait< ItemReference>(layer))
                                           .WithTrait(new GoalMarkerTrait< ItemReference, PerformanceGoal>(32)));

            queryRegistry = new SpatialQueryRegistry();
            queryRegistry.Register(new BruteForceSpatialQueryBackend<ItemReference>(entities.EntityRegistry));

            goalRegistry = new GoalRegistry();
            goalRegistry.RegisterGoalEntity<ItemReference, PerformanceGoal>();
            
            positions = new List<EntityGridPosition>();
            pathBuffer = new BufferList<(EntityGridPosition, IMovementMode)>(256);
            movementDataCollector = new MovementDataCollector();
            pathfinderSource = new SingleLevelGoalFinderSource(new SingleLevelGoalFinderPolicy(), goalRegistry, queryRegistry, movementDataCollector);
        }

        public virtual void SetUpGlobal()
        {
            var sourceText = PerformanceTestUtils.ReadResource(id);
            var movementCostData = PerformanceTestUtils.ParseMap(sourceText, out bounds);
            // Console.WriteLine("Using room layout \n" + TestHelpers.PrintMap(resistanceMap, bounds));
            Console.WriteLine("Producing layout for " + bounds);

            var directionalityMapSystem = new InboundMovementDirectionalitySystem<WalkingMovement>(movementCostData.As3DMap(0));
            directionalityMapSystem.MarkGloballyDirty();
            directionalityMapSystem.Process();
            if (!directionalityMapSystem.ResultView.TryGetView(0, out var directionalityMap))
                throw new Exception();

            movementDataCollector.RegisterMovementSource(WalkingMovement.Instance, movementCostData.As3DMap(0), directionalityMap.As3DMap(0), directionalityMap.As3DMap(0));

            var rnd = new Random(10);
            while (positions.Count < 50)
            {
                var startPosition = EntityGridPosition.OfRaw(layer.LayerId, rnd.Next(bounds.Width / 4, bounds.Width * 3 / 4), rnd.Next(bounds.Height / 4, bounds.Height * 3 / 4));
                if (positions.Contains(startPosition))
                {
                    continue;
                }

                if (movementCostData[startPosition.GridX, startPosition.GridY] > 0.5)
                {
                    positions.Add(startPosition);
                }
            }

            Console.WriteLine("Finished populating start positions");

            Console.WriteLine("Starting populating goal positions: " + new Rectangle(new Position2D(bounds.Width / 4, bounds.Height / 4),
                                                                                     new Position2D((bounds.Width * 3) / 4, (bounds.Height * 3) / 4)));
            var goalPositions = new HashSet<EntityGridPosition>();
            while (goalPositions.Count < 100)
            {
                var targetPosition = EntityGridPosition.OfRaw(layer.LayerId, 
                                                              rnd.Next(bounds.Width / 4, (bounds.Width * 3) / 4),
                                                              rnd.Next(bounds.Height / 4, (bounds.Height * 3) / 4));
                if (goalPositions.Contains(targetPosition))
                {
                    continue;
                }

                if (movementCostData[targetPosition.GridX, targetPosition.GridY] > 0.5)
                {
                    var ek = entities.ItemResolver.Instantiate( goalEntitiyId);
                    if (!entities.ItemResolver.TryUpdateData(ek,  targetPosition, out _))
                    {
                        entities.ItemResolver.TryUpdateData(ek,  targetPosition, out _);
                        throw new Exception($"Unable to position goal entity {positions.Count} at {targetPosition}");
                    }

                    goalPositions.Add(targetPosition);
                }
            }

            Console.WriteLine("Finished populating end positions");

            if (movementCostData[0, 9] < 1 ||
                movementCostData[0, 10] < 1 ||
                movementCostData[0, 11] < 1)
            {
                throw new Exception();
            }

            ValidateWorkingCondition();
        }

        void ValidateWorkingCondition()
        {
            var targetPosition = EntityGridPosition.OfRaw(layer.LayerId, 0, 11);
            var ek = entities.ItemResolver.Instantiate( goalEntitiyId);
            if (!entities.ItemResolver.TryUpdateData(ek,  targetPosition, out _))
            {
                entities.ItemResolver.TryUpdateData(ek,  targetPosition, out _);
                throw new Exception($"Unable to position goal entity {positions.Count} at {targetPosition}");
            }
            
            using (var pf = pathfinderSource.GetGoalFinder()
                                            .WithGoal<PerformanceGoal>()
                                            .Build(new PathfindingMovementCostFactors(new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1))))
            {
                var sourcePosition = EntityGridPosition.OfRaw(layer.LayerId, 0, 9);
                var result = pf.TryFindPath(sourcePosition, out var resultPath);
                if (result != PathFinderResult.Found)
                {
                    throw new ArgumentException("Unable to find sanity validation path");
                }

                //Console.WriteLine($" = {result2} + {string.Join(", ", resultPath2.Select(e => e.Item1))}");
                Console.WriteLine($"Found path => {string.Join(", ", resultPath.Select(e => e.Item1))}");
            }
        }
        
        public void ValidatePathFinding()
        {
            TimeSpan totalTime = TimeSpan.Zero;
            int nodesEvaluated = 0;
            int found = 0;
            int notFound = 0;

            var rnd = new Random(11);
            for (int i = 0; i < 100; i += 1)
            {
                var startPosition = positions[rnd.Next(0, 50)];

                using (var pf = pathfinderSource.GetGoalFinder()
                                                .WithGoal<PerformanceGoal>()
                                                .WithSearchRadius(32)
                                                .Build(new PathfindingMovementCostFactors(new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1))))
                {
                    if (i == 5440)
                    {
                        MemoryProfiler.CollectAllocations(true);
                        MemoryProfiler.GetSnapshot("Begin");
                    }

                    var result = pf.TryFindPath(startPosition, out _, pathBuffer);
                    if (result == PathFinderResult.Found)
                    {
                        found += 1;
                    }
                    if (result == PathFinderResult.NotFound)
                    {
                        notFound += 1;
                    }

                    if (pf is IPathFinderPerformanceView pv)
                    {
                        totalTime += pv.TimeElapsed;
                        nodesEvaluated += pv.NodesEvaluated;
                        // Console.WriteLine($"Performance View: {pv.NodesEvaluated:n0} in {pv.TimeElapsed}");
                    }

                    if (i == 5440)
                    {
                        MemoryProfiler.GetSnapshot("End");
                        MemoryProfiler.CollectAllocations(false);
                    }
                }
            }

            // Console.WriteLine($"Performance View: {nodesEvaluated:n0} nodes in {totalTime} sec (found: {found}; not found: {notFound}");
        }
    }
}
