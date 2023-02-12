using EnTTSharp.Entities;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;
using System;
using System.Collections.Generic;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using AABB = tainicom.Aether.Physics2D.Collision.AABB;

namespace RogueEntity.Physics2D.AetherPhysics;

/// <summary>
///   This class acts as spatial index for the AetherPhysics system. 
/// </summary>
public class AetherSharedWorlds
{
    readonly ObjectPool<AABBQuery> queryPool;
    readonly Dictionary<int, World> spatialIndex;

    public AetherSharedWorlds()
    {
        queryPool = new DefaultObjectPool<AABBQuery>(new DefaultPooledObjectPolicy<AABBQuery>());
        spatialIndex = new Dictionary<int, World>();
    }

    public void Clear()
    {
        foreach (var x in spatialIndex)
        {
            x.Value.Clear();
        }
    }

    public Dictionary<int,World>.ValueCollection Worlds => spatialIndex.Values;
    
    public World Get(int z)
    {
        if (this.spatialIndex.TryGetValue(z, out var world))
        {
            return world;
        }

        world = new World(Vector2.Zero);
        this.spatialIndex[z] = world;
        return world;
    }

    public BufferList<(int, World)> SpatialIndex(BufferList<(int, World)>? buffer = null)
    {
        buffer = BufferList.PrepareBuffer(buffer);
        foreach (var pair in spatialIndex)
        {
            buffer.Add((pair.Key, pair.Value));
        }

        return buffer;
    }

    public BufferList<(Fixture, TItemId)> Query<TItemId>(int gridZ,
                                                         BoundingBox bb,
                                                         BufferList<(Fixture, TItemId)> buffer)
        where TItemId : struct, IEntityKey
    {
        buffer = BufferList.PrepareBuffer(buffer);
        if (!spatialIndex.TryGetValue(gridZ, out var world))
        {
            return buffer;
        }


        var aabb = new AABB(new Vector2(bb.Left, bb.Top), new Vector2(bb.Right, bb.Bottom));
        var query = queryPool.Get();
        try
        {
            foreach (var f in query.Query(world, ref aabb))
            {
                if (f.Body.Tag is TItemId item)
                {
                    buffer.Add((f, item));
                }
            }
        }
        finally
        {
            query.Clear();
            queryPool.Return(query);
        }

        return buffer;
    }

    /// <summary>
    ///    This class is guaranteed to be only used in one thread.
    /// </summary>
    class AABBQuery
    {
        readonly BufferList<Fixture> capturedFixtures;
        readonly QueryReportFixtureDelegate queryReportFixtureDelegate;
        AABB queryBox;

        public AABBQuery()
        {
            this.queryBox = default;
            this.queryReportFixtureDelegate = Test;
            this.capturedFixtures = new BufferList<Fixture>();
        }

        public void Clear()
        {
            capturedFixtures.Clear();
        }

        public BufferList<Fixture> Query(World w, ref AABB q)
        {
            capturedFixtures.Clear();
            queryBox = q;
            w.QueryAABB(queryReportFixtureDelegate, ref queryBox);
            return capturedFixtures;
        }

        bool Test(Fixture f)
        {
            for (int p = 0; p < f.ProxyCount; p++)
            {
                f.GetAABB(out var aabb, p);
                if (AABB.TestOverlap(ref queryBox, ref aabb))
                {
                    capturedFixtures?.Add(f);
                    break;
                }
            }

            return true;
        }
    }

    public void ProcessWorldStep(TimeSpan currentFixedTime)
    {
        foreach (var world in spatialIndex.Values)
        {
            world.Step(currentFixedTime);
        }
    }
}