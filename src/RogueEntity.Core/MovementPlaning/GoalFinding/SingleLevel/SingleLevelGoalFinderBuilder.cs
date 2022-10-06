using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.MovementPlaning.Goals;
using RogueEntity.Core.MovementPlaning.Goals.Filters;
using RogueEntity.Core.Utils;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.GoalFinding.SingleLevel
{
    public class SingleLevelGoalFinderBuilder: IGoalFinderBuilder, IGoalLift
    {
        readonly ObjectPool<SingleLevelGoalFinder> pathFinderPool;
        readonly ObjectPool<CompoundGoalTargetSource> compoundTargetEvaluatorPool;
        readonly ObjectPool<AnyOfGoalFinderFilter> filterPool;

        readonly List<IGoalFinderTargetSource> goalSources;
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
            this.goalSources = new List<IGoalFinderTargetSource>();
        }

        public void Configure(IReadOnlyDictionary<IMovementMode, MovementSourceData> data)
        {
            MovementCostData = data ?? throw new ArgumentNullException(nameof(data));
            searchRadius = 16;
        }
        
        IReadOnlyDictionary<IMovementMode, MovementSourceData>? MovementCostData { get; set; }

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

        public IGoalFinder Build(in AggregateMovementCostFactors movementProfile)
        {
            Assert.NotNull(MovementCostData);
            
            var e = compoundTargetEvaluatorPool.Get();
            foreach (var goal in goalSources)
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
                    g.ConfigureMovementProfile(m, mapData.Costs, mapData.InboundDirections, mapData.OutboundDirections);
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

        void IGoalLift.Invoke<TEntityKey, TGoal>()
        {
            if (evaluatorFactory.TryGet<TEntityKey, TGoal>(out var q))
            {
                goalSources.Add(q);
            }
        }

        public void Reset()
        {
            goalSources.Clear();
        }
    }
}