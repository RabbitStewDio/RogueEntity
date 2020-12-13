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
        int CollectGoals(in Position origin, float range, DistanceCalculation dc, IGoalFinderTargetEvaluatorVisitor v);
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
        
        public int CollectGoals(in Position origin, float range, DistanceCalculation dc, IGoalFinderTargetEvaluatorVisitor v)
        {
            var goalsCollected = 0;
            foreach (var f in entityTypeGoalFinders)
            {
                goalsCollected += f.CollectGoals(origin, range, dc, v);
            }

            return goalsCollected;
        }
    }
    
    public class GoalTargetEvaluator2D<TItemId, TGoal> : IGoalFinderTargetEvaluator
        where TItemId : IEntityKey
    {
        readonly ISpatialQuery<TItemId> query;
        readonly List<SpatialQueryResult<TItemId, GoalMarker<TGoal>>> buffer;

        public GoalTargetEvaluator2D(ISpatialQuery<TItemId> query)
        {
            this.query = query;
            this.buffer = new List<SpatialQueryResult<TItemId, GoalMarker<TGoal>>>();
        }

        public void Dispose()
        {
        }

        public int CollectGoals(in Position origin, float range, DistanceCalculation dc, IGoalFinderTargetEvaluatorVisitor v)
        {
            int goalsCollected = 0;
            query.Query2D(origin, range, dc, buffer);
            
            for (var i = 0; i < buffer.Count; i++)
            {
                var c = buffer[i];
                v.RegisterGoalAt(c.Position, c.Component);
                goalsCollected += 1;
            }
            return goalsCollected;
        }
    }

    public class GoalTargetEvaluator3D<TItemId, TGoal> : IGoalFinderTargetEvaluator
        where TItemId : IEntityKey
    {
        readonly List<SpatialQueryResult<TItemId, GoalMarker<TGoal>>> buffer;
        readonly ISpatialQuery<TItemId> query;

        public GoalTargetEvaluator3D(ISpatialQuery<TItemId> query)
        {
            this.query = query;
            this.buffer = new List<SpatialQueryResult<TItemId, GoalMarker<TGoal>>>();
        }

        public void Dispose()
        {
        }

        public int CollectGoals(in Position origin, float range, DistanceCalculation dc, IGoalFinderTargetEvaluatorVisitor v)
        {
            int goalsCollected = 0;
            query.Query3D(origin, range, dc, buffer);

            for (var i = 0; i < buffer.Count; i++)
            {
                var c = buffer[i];
                v.RegisterGoalAt(c.Position, c.Component);
                goalsCollected += 1;
            }
            return goalsCollected;
        }
    }
}