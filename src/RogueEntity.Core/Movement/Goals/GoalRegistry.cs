using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Movement.Goals
{
    public class GoalRegistry
    {
        readonly Dictionary<Type, IGoalEntry> registeredLifters;

        public GoalRegistry()
        {
            registeredLifters = new Dictionary<Type, IGoalEntry>();
        }

        public void RegisterGoalEntity<TItemId, TGoal>()
            where TItemId : IEntityKey
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

        public void Lift<TGoal>(IGenericLifter<IEntityKey, IGoal> g)
            where TGoal : IGoal
        {
            if (registeredLifters.TryGetValue(typeof(TGoal), out var entryRaw))
            {
                entryRaw.InvokeAll(g);
            }
        }

        public object LiftInstance(Type entity, Type goal, IGenericLifterFunction<IEntityKey, IGoal> g)
        {
            if (registeredLifters.TryGetValue(goal, out var entryRaw))
            {
                return entryRaw.LiftInstance(entity, g);
            }

            return null;
        }

        interface IGoalEntry
        {
            void InvokeAll(IGenericLifter<IEntityKey, IGoal> g);
            object LiftInstance(Type entity, IGenericLifterFunction<IEntityKey, IGoal> genericLifter);
        }

        class GoalEntry<TGoal> : IGoalEntry
            where TGoal : IGoal
        {
            readonly Dictionary<Type, Action<IGenericLifter<IEntityKey, IGoal>>> lifterActions;
            readonly Dictionary<Type, Func<IGenericLifterFunction<IEntityKey, IGoal>, object>> lifterFunctions;

            public GoalEntry()
            {
                lifterActions = new Dictionary<Type, Action<IGenericLifter<IEntityKey, IGoal>>>();
                lifterFunctions = new Dictionary<Type, Func<IGenericLifterFunction<IEntityKey, IGoal>, object>>();
            }

            public void AddEntity<TItemId>()
                where TItemId : IEntityKey
            {
                lifterActions[typeof(TItemId)] = CallLift<TItemId>;
                lifterFunctions[typeof(TItemId)] = CallLiftFunc<TItemId>;
            }

            void CallLift<TItemId>(IGenericLifter<IEntityKey, IGoal> d)
                where TItemId : IEntityKey
                => d.Invoke<TItemId, TGoal>();

            object CallLiftFunc<TItemId>(IGenericLifterFunction<IEntityKey, IGoal> d)
                where TItemId : IEntityKey
                => d.Invoke<TItemId, TGoal>();

            public void InvokeAll(IGenericLifter<IEntityKey, IGoal> g)
            {
                foreach (var action in lifterActions.Values)
                {
                    action(g);
                }
            }

            public object LiftInstance(Type entity, IGenericLifterFunction<IEntityKey, IGoal> genericLifter)
            {
                if (lifterFunctions.TryGetValue(entity, out var entry))
                {
                    return entry(genericLifter);
                }

                return null;
            }
        }
    }

    public interface IGoal
    {
    }
}