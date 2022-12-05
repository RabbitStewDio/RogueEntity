using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
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
            if (q == null)
            {
                throw new ArgumentNullException(nameof(q));
            }

            if (!backend.TryGetValue(typeof(TEntityKey), out var existing))
            {
                backend[typeof(TEntityKey)] = q;
                return;
            }

            if (existing is AggregateSpatialQuery<TEntityKey> agg)
            {
                agg.Add(q);
                return;
            }

            agg = new AggregateSpatialQuery<TEntityKey>();
            agg.Add((ISpatialQuery<TEntityKey>)existing);
            agg.Add(q);
            backend[typeof(TEntityKey)] = agg;
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

        class AggregateSpatialQuery<TEntityKey> : ISpatialQuery<TEntityKey> 
            where TEntityKey : struct, IEntityKey
        {
            readonly List<ISpatialQuery<TEntityKey>> queryBackends;

            public AggregateSpatialQuery()
            {
                this.queryBackends = new List<ISpatialQuery<TEntityKey>>();
            }

            public void Add(ISpatialQuery<TEntityKey> q)
            {
                this.queryBackends.Add(q);
            }

            public BufferList<SpatialQueryResult<TEntityKey, TComponent>> QueryBox<TComponent>(in Rectangle3D queryRegion, 
                                                                                               BufferList<SpatialQueryResult<TEntityKey, TComponent>>? buffer = null)
            {
                buffer = BufferList.PrepareBuffer(buffer);
                using var b = BufferListPool<SpatialQueryResult<TEntityKey, TComponent>>.GetPooled();
                foreach (var q in queryBackends)
                {
                    b.Data.Clear();
                    q.QueryBox<TComponent>(queryRegion, b);
                    b.Data.CopyToBuffer(buffer);
                }

                return buffer;
            }

            public BufferList<SpatialQueryResult<TEntityKey, TComponent>> QuerySphere<TComponent>(in Position pos, 
                                                                                                  float distance = 1, 
                                                                                                  DistanceCalculation d = DistanceCalculation.Euclid, 
                                                                                                  BufferList<SpatialQueryResult<TEntityKey, TComponent>>? buffer = null)
            {
                buffer = BufferList.PrepareBuffer(buffer);
                using var b = BufferListPool<SpatialQueryResult<TEntityKey, TComponent>>.GetPooled();
                foreach (var q in queryBackends)
                {
                    b.Data.Clear();
                    q.QuerySphere<TComponent>(pos, distance, d, b);
                    b.Data.CopyToBuffer(buffer);
                }

                return buffer;
            }
        }
    }
}