using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Positioning.SpatialQueries
{
    public class SpatialQueryRegistry : ISpatialQueryLookup
    {
        readonly Dictionary<Type, object> backend;

        public SpatialQueryRegistry()
        {
            backend = new Dictionary<Type, object>();
        }

        public void Register<TEntityKey>(ISpatialQuery<TEntityKey> q)
            where TEntityKey : struct, IEntityKey
        {
            backend[typeof(TEntityKey)] = q ?? throw new ArgumentNullException(nameof(q));
        }

        public bool TryGetQuery<TEntityKey>([MaybeNullWhen(false)] out ISpatialQuery<TEntityKey> q)
            where TEntityKey : struct, IEntityKey
        {
            if (backend.TryGetValue(typeof(TEntityKey), out var raw) &&
                raw is ISpatialQuery<TEntityKey> qq)
            {
                q = qq;
                return true;
            }

            q = default;
            return false;
        }
    }
}