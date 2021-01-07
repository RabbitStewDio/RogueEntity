using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.Goals;
using RogueEntity.Core.Movement.Pathfinding;

namespace RogueEntity.Core.Movement.GoalFinding.SingleLevel
{
    public class SingleLevelGoalFinderBuilder: IGoalFinderBuilder, IGenericLifter<IEntityKey, IGoal>
    {
        readonly ObjectPool<SingleLevelGoalFinder> pathFinderPool;
        readonly ObjectPool<CompoundGoalTargetSource> compoundTargetEvaluatorPool;
        readonly ObjectPool<AnyOfGoalFinderFilter> filterPool;

        readonly List<IGoalFinderTargetSource> goals;
        readonly SingleLevelGoalTargetEvaluatorFactory evaluatorFactory;
        readonly GoalRegistry goalRegistry;
        readonly List<IGoalFinderFilter> filters;
        float searchRadius;

        public SingleLevelGoalFinderBuilder(SingleLevelGoalTargetEvaluatorFactory evaluatorFactory, 
                                            GoalRegistry goalRegistry,
                                            ObjectPool<SingleLevelGoalFinder> pathFinderPool,
                                            ObjectPool<CompoundGoalTargetSource> compoundTargetEvaluatorPool,
                                            ObjectPool<AnyOfGoalFinderFilter> filterPool)
        {
            this.evaluatorFactory = evaluatorFactory;
            this.goalRegistry = goalRegistry;
            this.pathFinderPool = pathFinderPool;
            this.compoundTargetEvaluatorPool = compoundTargetEvaluatorPool;
            this.filterPool = filterPool;
            this.filters = new List<IGoalFinderFilter>();
            this.goals = new List<IGoalFinderTargetSource>();
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

        public IGoalFinderBuilder WithFilter(IGoalFinderFilter filter)
        {
            filters.Add(filter);
            return this;
        }
        
        public IGoalFinderBuilder WithSearchRadius(float radius)
        {
            this.searchRadius = radius;
            return this;
        }

        public IGoalFinder Build(in PathfindingMovementCostFactors movementProfile)
        {
            var e = compoundTargetEvaluatorPool.Get();
            foreach (var goal in goals)
            {
                e.Add(goal);
            }

            var filter = CreateGoalFinderFilter();

            var g = pathFinderPool.Get();
            g.Configure(this, e, filter, searchRadius);
            foreach (var m in movementProfile.MovementCosts)
            {
                if (MovementCostData.TryGetValue(m.MovementMode, out var mapData))
                {
                    g.ConfigureMovementProfile(m, mapData.Costs, mapData.Directions);
                }
            }
        
            return g;

        }

        IGoalFinderFilter CreateGoalFinderFilter()
        {
            IGoalFinderFilter filter;
            if (filters.Count > 0)
            {
                var f = filterPool.Get();
                foreach (var fi in filters)
                {
                    f.With(fi);
                }

                filter = f;
            }
            else
            {
                filter = PassThroughGoalFinderFilter.Instance;
            }

            return filter;
        }

        public void Return(SingleLevelGoalFinder pf)
        {
            pf.TargetSource?.Dispose();
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