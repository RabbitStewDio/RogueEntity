using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.SpatialQueries;

namespace RogueEntity.Core.Movement.GoalFinding
{
    public interface IGoalFinderTargetEvaluatorVisitor
    {
        public void RegisterGoalAt<TGoal>(in Position pos, GoalMarker<TGoal> goal);
    }

    public interface IGoalFinderTargetEvaluator: IDisposable
    {
        void CollectGoals(in Position origin, float range, DistanceCalculation dc, IGoalFinderTargetEvaluatorVisitor v);
    }

    public class CompoundGoalTargetEvaluator : IGoalFinderTargetEvaluator
    {
        readonly List<IGoalFinderTargetEvaluator> entityTypeGoalFinders;

        public CompoundGoalTargetEvaluator()
        {
            entityTypeGoalFinders = new List<IGoalFinderTargetEvaluator>();
        }

        public void Dispose()
        {
            foreach (var e in entityTypeGoalFinders)
            {
                e.Dispose();
            }
            entityTypeGoalFinders.Clear();
        }

        public void Add(IGoalFinderTargetEvaluator e)
        {
            entityTypeGoalFinders.Add(e);
        }
        
        public void CollectGoals(in Position origin, float range, DistanceCalculation dc, IGoalFinderTargetEvaluatorVisitor v)
        {
            foreach (var f in entityTypeGoalFinders)
            {
                f.CollectGoals(origin, range, dc, v);
            }
        }
    }
    
    public class GoalTargetEvaluator2D<TItemId, TGoal> : IGoalFinderTargetEvaluator
        where TItemId : IEntityKey
    {
        readonly ISpatialQuery<TItemId> query;

        public GoalTargetEvaluator2D(ISpatialQuery<TItemId> query)
        {
            this.query = query;
        }

        public void Dispose()
        {
        }

        public void CollectGoals(in Position origin, float range, DistanceCalculation dc, IGoalFinderTargetEvaluatorVisitor v)
        {
            void ReceiveSpatialQueryResult(in SpatialQueryResult<TItemId, GoalMarker<TGoal>> c)
            {
                v.RegisterGoalAt(c.Position, c.Component);
            }

            query.Query2D<GoalMarker<TGoal>>(ReceiveSpatialQueryResult, origin, range, dc);
        }
    }

    public class GoalTargetEvaluator3D<TItemId, TGoal> : IGoalFinderTargetEvaluator
        where TItemId : IEntityKey
    {
        readonly ISpatialQuery<TItemId> query;

        public GoalTargetEvaluator3D(ISpatialQuery<TItemId> query)
        {
            this.query = query;
        }

        public void Dispose()
        {
        }

        public void CollectGoals(in Position origin, float range, DistanceCalculation dc, IGoalFinderTargetEvaluatorVisitor v)
        {
            void ReceiveSpatialQueryResult(in SpatialQueryResult<TItemId, GoalMarker<TGoal>> c)
            {
                v.RegisterGoalAt(c.Position, c.Component);
            }

            query.Query3D<GoalMarker<TGoal>>(ReceiveSpatialQueryResult, origin, range, dc);
        }
    }
}