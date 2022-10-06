using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RogueEntity.Core.MovementPlaning.Goals
{
    public class GoalSet
    {
        static readonly ThreadLocal<Dictionary<Position, float>> intersectionBuffer = new ThreadLocal<Dictionary<Position, float>>(() => new Dictionary<Position, float>());
        readonly Dictionary<Position, float> goals;

        public GoalSet()
        {
            goals = new Dictionary<Position, float>();
        }

        public void Clear()
        {
            goals.Clear();
        }

        public bool Add(in Position p, float str)
        {
            if (goals.TryGetValue(p, out var strExisting))
            {
                if (strExisting >= str)
                {
                    return false;
                }
            }

            goals[p] = str;
            return true;
        }

        public int Count => goals.Count;
        
        public bool Add(in GoalRecord goal)
        {
            return Add(goal.Position, goal.Strength);
        }

        public BufferList<GoalRecord> CopyTo(BufferList<GoalRecord> buffer)
        {
            buffer = BufferList.PrepareBuffer(buffer);
            buffer.EnsureCapacity(goals.Count);

            foreach (var pair in goals)
            {
                buffer.Add(new GoalRecord(pair.Value, pair.Key));
            }

            return buffer;
        }
        
        public static GoalSet PrepareBuffer(GoalSet? receiver)
        {
            if (receiver == null)
            {
                return new GoalSet();
            }

            receiver.Clear();
            return receiver;
        }

        public void Union(GoalSet workingSet)
        {
            foreach (var pair in workingSet.goals)
            {
                Add(pair.Key, pair.Value);
            }
        }

        public void Intersect(GoalSet workingSet)
        {
            var tmp = intersectionBuffer.Value; 
            foreach (var pair in goals)
            {
                if (!workingSet.goals.TryGetValue(pair.Key, out var wsStr))
                {
                    continue;
                }

                tmp[pair.Key] = Math.Max(wsStr, pair.Value);
            }
            goals.Clear();

            foreach (var t in tmp)
            {
                goals.Add(t.Key, t.Value);
            }
            
            tmp.Clear();
        }
    }
}
