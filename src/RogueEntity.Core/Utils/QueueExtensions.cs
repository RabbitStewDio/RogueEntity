using System.Collections.Generic;

namespace RogueEntity.Core.Utils
{
    public static class QueueExtensions
    {
        public static bool TryDequeue<T>(this Queue<T> q, out T value)
        {
            if (q.Count > 0)
            {
                value = q.Dequeue();
                return true;
            }

            value = default(T);
            return false;
        }
    }
}