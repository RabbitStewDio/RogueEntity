using EnTTSharp;
using EnTTSharp.Entities;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Goals
{
    public interface IGoalLift
    {
        public void Invoke<TItemId, TGoal>()
            where TItemId : struct, IEntityKey
            where TGoal : IGoal;
    }
    
    public interface IGoalLiftFunc
    {
        public T Invoke<TItemId, TGoal, T>()
            where TItemId : struct, IEntityKey
            where TGoal : IGoal;
    }
    
    public class GoalRegistry
    {
        readonly Dictionary<Type, IGoalEntry> registeredLifters;

        public GoalRegistry()
        {
            registeredLifters = new Dictionary<Type, IGoalEntry>();
        }

        public void RegisterGoalEntity<TItemId, TGoal>()
            where TItemId : struct, IEntityKey
            where TGoal : IGoal
        {
            if (!registeredLifters.TryGetValue(typeof(TGoal), out var entryRaw) ||
                !(entryRaw is GoalEntry<TGoal> entry))
            {
                entry = new GoalEntry<TGoal>();
                registeredLifters[typeof(TGoal)] = entry;
            }

            entry.AddEntity<TItemId>();
        }

        public void Lift<TGoal>(IGoalLift g)
            where TGoal : IGoal
        {
            if (registeredLifters.TryGetValue(typeof(TGoal), out var entryRaw))
            {
                entryRaw.InvokeAll(g);
            }
        }

        public Optional<TResult> LiftInstance<TResult>(Type entity, Type goal, IGoalLiftFunc g)
        {
            if (registeredLifters.TryGetValue(goal, out var entryRaw))
            {
                return entryRaw.LiftInstance<TResult>(entity, g);
            }

            return default;
        }

        interface IGoalEntry
        {
            void InvokeAll(IGoalLift g);
            Optional<T> LiftInstance<T>(Type entity, IGoalLiftFunc genericLifter);
        }

        interface IInvokeHelper
        {
            Optional<T> Invoke<T>(IGoalLiftFunc f);
        }

        class GoalFunctor<TItemId, TGoal>: IInvokeHelper
            where TGoal : IGoal
            where TItemId: struct, IEntityKey
        {
            public Optional<T> Invoke<T>(IGoalLiftFunc f) => f.Invoke<TItemId, TGoal, T>();
        }
        
        class GoalEntry<TGoal> : IGoalEntry
            where TGoal : IGoal
        {
            readonly Dictionary<Type, Action<IGoalLift>> lifterActions;
            readonly Dictionary<Type, IInvokeHelper> lifterFunctions;

            public GoalEntry()
            {
                lifterActions = new Dictionary<Type, Action<IGoalLift>>();
                lifterFunctions = new Dictionary<Type, IInvokeHelper>();
            }

            public void AddEntity<TItemId>()
                where TItemId : struct, IEntityKey
            {
                lifterActions[typeof(TItemId)] = CallLiftAction<TItemId>;
                lifterFunctions[typeof(TItemId)] = new GoalFunctor<TItemId, TGoal>();
            }

            void CallLiftAction<TItemId>(IGoalLift d)
                where TItemId : struct, IEntityKey
                => d.Invoke<TItemId, TGoal>();

            public void InvokeAll(IGoalLift g)
            {
                foreach (var action in lifterActions.Values)
                {
                    action(g);
                }
            }

            public Optional<T> LiftInstance<T>(Type entity, IGoalLiftFunc genericLifter)
            {
                if (lifterFunctions.TryGetValue(entity, out var entry))
                {
                    return entry.Invoke<T>(genericLifter);
                }

                return default;
            }
        }
    }

    public interface IGoal
    {
    }
}