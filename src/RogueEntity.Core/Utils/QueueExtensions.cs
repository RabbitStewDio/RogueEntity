using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils
{
    public static class QueueExtensions
    {
        public static bool TryDequeue<T>(this Queue<T> q, [MaybeNullWhen(false)] out T value)
        {
            if (q.Count > 0)
            {
                value = q.Dequeue();
                return true;
            }

            value = default;
            return false;
        }
    }
}