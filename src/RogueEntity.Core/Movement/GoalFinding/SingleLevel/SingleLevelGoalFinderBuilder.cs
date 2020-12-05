using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.Pathfinding;

namespace RogueEntity.Core.Movement.GoalFinding.SingleLevel
{
    public class SingleLevelGoalFinderBuilder: IGoalFinderBuilder, IGenericLifter<IEntityKey, IGoal>
    {
        readonly ObjectPool<SingleLevelGoalFinder> pathFinderPool;
        readonly ObjectPool<CompoundGoalTargetEvaluator> compoundTargetEvaluatorPool;

        readonly List<IGoalFinderTargetEvaluator> goals;
        readonly SingleLevelGoalTargetEvaluatorFactory evaluatorFactory;
        readonly GoalRegistry goalRegistry;
        float searchRadius;

        public SingleLevelGoalFinderBuilder(SingleLevelGoalTargetEvaluatorFactory evaluatorFactory, 
                                            GoalRegistry goalRegistry,
                                            ObjectPool<SingleLevelGoalFinder> pathFinderPool,
                                            ObjectPool<CompoundGoalTargetEvaluator> compoundTargetEvaluatorPool)
        {
            this.evaluatorFactory = evaluatorFactory;
            this.goalRegistry = goalRegistry;
            this.pathFinderPool = pathFinderPool;
            this.compoundTargetEvaluatorPool = compoundTargetEvaluatorPool;
            this.goals = new List<IGoalFinderTargetEvaluator>();
        }

        public void Configure([NotNull] IReadOnlyDictionary<IMovementMode, SingleLevelGoalFinderSource.MovementSourceData> data)
        {
            MovementCostData = data ?? throw new ArgumentNullException(nameof(data));
            searchRadius = 16;
        }
        
        IReadOnlyDictionary<IMovementMode, SingleLevelGoalFinderSource.MovementSourceData> MovementCostData { get; set; }

        public IGoalFinderBuilder WithGoal<TGoal>()
            where TGoal : IGoal
        {
            goalRegistry.Lift<TGoal>(this);
            return this;
        }

        public IGoalFinderBuilder WithSearchRadius(float radius)
        {
            this.searchRadius = radius;
            return this;
        }

        public IPathFinder Build(in PathfindingMovementCostFactors movementProfile)
        {
            var e = compoundTargetEvaluatorPool.Get();
            foreach (var goal in goals)
            {
                e.Add(goal);
            }
            
            var g = pathFinderPool.Get();
            g.Configure(this, e, searchRadius);
            foreach (var m in movementProfile.MovementCosts)
            {
                if (MovementCostData.TryGetValue(m.MovementMode, out var mapData))
                {
                    g.ConfigureMovementProfile(m, mapData.Costs, mapData.Directions);
                }
            }
        
            return g;

        }

        public void Return(SingleLevelGoalFinder pf)
        {
            pf.TargetEvaluator?.Dispose();
            pathFinderPool.Return(pf);
        }

        void IGenericLifter<IEntityKey, IGoal>.Invoke<TEntityKey, TGoal>()
        {
            if (evaluatorFactory.TryGet<TEntityKey, TGoal>(out var q))
            {
                goals.Add(q);
            }
        }

        public void Reset()
        {
            goals.Clear();
        }
    }
}