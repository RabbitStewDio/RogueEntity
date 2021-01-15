using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Core.Infrastructure.Actions.Schedule
{
    public class ScheduledActionPlanFormatterResolver : IFormatterResolver
    {
        readonly Dictionary<Type, object> cache;

        public ScheduledActionPlanFormatterResolver()
        {
            cache = new Dictionary<Type, object>();
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            if (cache.TryGetValue(typeof(T), out var existing))
            {
                return (IMessagePackFormatter<T>) existing;
            }

            if (typeof(T).GetGenericTypeDefinition() != typeof(ScheduledActionPlan<>))
            {
                return null;
            }

            var genericArgs = typeof(T).GetGenericArguments();
            var formatter = typeof(ScheduledActionPlanMessageFormatter<>).MakeGenericType(genericArgs);
            var instance = Activator.CreateInstance(formatter);
            cache[typeof(T)] = instance;
            return (IMessagePackFormatter<T>)instance;
        }
    }
}