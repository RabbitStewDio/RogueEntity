using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils
{
    public class TypedRuleProperties
    {
        readonly Dictionary<Type, object> properties;

        public TypedRuleProperties()
        {
            properties = new Dictionary<Type, object>();
        }

        public void Define<T>(T property)
        {
            properties[typeof(T)] = property;
        }

        public bool TryGet<T>(out T data)
        {
            if (properties.TryGetValue(typeof(T), out object raw))
            {
                data = (T) raw;
                return true;
            }

            data = default;
            return false;
        }
    }
}