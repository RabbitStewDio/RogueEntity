using MessagePack;
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

        [SerializationConstructor]
        internal TypedRuleProperties(Dictionary<Type, object> properties)
        {
            this.properties = properties;
        }

        public void Define<T>(T property)
        {
            properties[typeof(T)] = property;
        }

        public TypedRuleProperties With<T>(T property)
        {
            var copy = Copy();
            copy.properties[typeof(T)] = property;
            return copy;
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

        public TypedRuleProperties Copy()
        {
            return new TypedRuleProperties(new Dictionary<Type, object>(properties));
        }
    }
}