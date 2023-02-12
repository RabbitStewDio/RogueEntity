using EnTTSharp;
using EnTTSharp.Entities;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.SpatialQueries;
using RogueEntity.Core.Utils;
using RogueEntity.Physics2D.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;

namespace RogueEntity.Physics2D.AetherPhysics;

/**
 * 
 */
public class AetherPhysicsSystem<TItemId>
    where TItemId : struct, IEntityKey
{
    readonly IItemResolver<TItemId> itemResolver;
    readonly IMapContext<TItemId> mapContext;
    readonly IItemPlacementServiceContext<TItemId> placementServiceContext;
    readonly ObjectPool<BodyAttachment> bodyAttachmentPool;

    [SuppressMessage("ReSharper", "CollectionNeverQueried.Local")]
    readonly List<PhysicsBodyTracker> trackers;

    readonly AetherSharedWorlds worlds;
    readonly Lazy<ITimeSource> timeSource;
    Optional<GameTimeState> lastFrameStateHolder;

    public AetherPhysicsSystem(IItemResolver<TItemId> itemResolver,
                               IItemPlacementServiceContext<TItemId> placementServiceContext,
                               IMapContext<TItemId> mapContext,
                               Lazy<ITimeSource> timeSource)
    {
        this.itemResolver = itemResolver;
        this.placementServiceContext = placementServiceContext;
        this.mapContext = mapContext;
        this.timeSource = timeSource;
        this.worlds = new AetherSharedWorlds();
        this.trackers = new List<PhysicsBodyTracker>();
        this.bodyAttachmentPool = new DefaultObjectPool<BodyAttachment>(new DefaultPooledObjectPolicy<BodyAttachment>(), 256_000);
    }

    public void ProcessDestroyedItems(IEntityViewControl<TItemId> v,
                                      TItemId k,
                                      in DestroyedMarker _,
                                      in AetherBodyTracker tracker)
    {
        if (!tracker.TryGetBody(out var body))
        {
            return;
        }

        foreach (var fixture in body.FixtureList)
        {
            body.Remove(fixture);
        }

        body.World.Remove(body);
    }

    public void DeclareCollision<TOtherId>(ISpatialQuery<TOtherId, IPhysicsShape2D> otherQuery)
        where TOtherId : struct, IEntityKey
    {
    }

    public void Init()
    {
        trackers.Clear();
        foreach (var layer in mapContext.Layers())
        {
            if (placementServiceContext.TryGetItemPlacementService(layer, out var placementService))
            {
                trackers.Add(new PhysicsBodyTracker(bodyAttachmentPool, placementService, itemResolver, worlds));
            }
        }

        lastFrameStateHolder = default;
    }

    public void ProcessWorldStep()
    {
        if (!lastFrameStateHolder.TryGetValue(out var lastFrameState))
        {
            lastFrameStateHolder = timeSource.Value.TimeState;
            return;
        }

        var currentState = timeSource.Value.TimeState;
        var lastFixedTime = lastFrameState.FixedGameTimeElapsed;
        var currentFixedTime = currentState.FixedGameTimeElapsed;
        if (currentFixedTime <= lastFixedTime)
        {
            return;
        }

        lastFrameStateHolder = timeSource.Value.TimeState;
        worlds.ProcessWorldStep(currentFixedTime - lastFixedTime);

        // now check all dynamic bodies to see whether they have moved.
        foreach (var t in trackers)
        {
            t.SuspendEventHandling = true;
        }
    }

    public void UpdatePositions(IEntityViewControl<TItemId> v,
                                TItemId k,
                                in AetherBodyTracker tracker,
                                in ContinuousMapPosition pos)
    {
        if (!tracker.TryGetBody(out var body))
        {
            return;
        }

        if (pos.IsInvalid)
        {
            // this body should not be active in the world.
            return;
        }

        if (!placementServiceContext.TryGetItemPlacementService(pos.LayerId, out var placementService))
        {
            return;
        }

        if (body.Tag is not BodyAttachment ba || !ba.currentRegion.TryGetValue(out var region))
        {
            return;
        }

        var position = body.Position;
        var nextPos = ContinuousMapPosition.Of(region.layerId, position.X, position.Y, region.z);
        if (nextPos != pos)
        {
            placementService.TryMoveItem(k, pos, nextPos);
        }
    }

    public void UpdatePositions(IEntityViewControl<TItemId> v,
                                TItemId k,
                                in AetherBodyTracker tracker,
                                ref EntityGridPosition pos)
    {
        if (!tracker.TryGetBody(out var body))
        {
            return;
        }

        if (pos.IsInvalid)
        {
            // this body should not be active in the world.
            return;
        }

        if (!placementServiceContext.TryGetItemPlacementService(pos.LayerId, out var placementService))
        {
            return;
        }

        if (body.Tag is not BodyAttachment ba || !ba.currentRegion.TryGetValue(out var region))
        {
            return;
        }

        var position = body.Position;
        var nextPos = EntityGridPosition.Of(region.layerId, position.X, position.Y, region.z);
        if (nextPos != pos)
        {
            placementService.TryMoveItem(k, pos, nextPos);
        }
    }

    public void PostProcessWorldStep()
    {
        // update positions ..

        foreach (var t in trackers)
        {
            t.SuspendEventHandling = false;
        }
    }

    class PhysicsBodyTracker : IBodyTracker
    {
        readonly IItemResolver<TItemId> itemResolver;
        readonly AetherSharedWorlds worlds;
        readonly ObjectPool<BodyAttachment> attachmentPool;
        readonly IItemPlacementService<TItemId> placementContext;

        public PhysicsBodyTracker(ObjectPool<BodyAttachment> attachmentPool,
                                  IItemPlacementService<TItemId> placementContext,
                                  IItemResolver<TItemId> itemResolver,
                                  AetherSharedWorlds worlds)
        {
            this.attachmentPool = attachmentPool;
            this.placementContext = placementContext;
            this.itemResolver = itemResolver;
            this.worlds = worlds;
            this.placementContext.ItemPositionChanged += OnItemPositionChanged;
        }

        public bool SuspendEventHandling { get; set; }

        void OnItemPositionChanged(object sender, ItemPositionChangedEvent<TItemId> e)
        {
            if (SuspendEventHandling)
            {
                return;
            }

            if (e.TargetPosition.IsInvalid)
            {
                if (e.SourcePosition.IsInvalid)
                {
                    // invalid
                    return;
                }

                RemoveBody(e);
                return;
            }

            if (e.SourcePosition.IsInvalid)
            {
                InsertBody(e);
            }
        }

        void InsertBody(ItemPositionChangedEvent<TItemId> e)
        {
            var z = e.TargetPosition.GridZ;
            var world = worlds.Get(z);

            if (itemResolver.TryQueryData<AetherBodyTracker>(e.Item, out var bodyTracker) &&
                bodyTracker.TryGetBody(out var existingBody))
            {
                if (existingBody.Tag is BodyAttachment ba)
                {
                    ba.currentRegion = (e.TargetPosition.LayerId, z);
                }

                world.Add(existingBody);
                return;
            }

            if (!itemResolver.TryQueryData<PhysicsBody2D>(e.Item, out var bodyDef))
            {
                return;
            }


            var aetherBody = world.CreateBody(e.TargetPosition.ToAether(), 0, bodyDef.BodyType.ToAether());
            using var buffer = BufferListPool<Shape>.GetPooled();
            TryCreateShape(bodyDef.Shape, Transform.Identity, buffer);
            foreach (var shape in buffer.Data)
            {
                aetherBody.CreateFixture(shape);
            }

            if (itemResolver.EntityMetaData.IsReferenceEntity(e.Item))
            {
                var fixtureTracker = new AetherBodyTracker(aetherBody);
                itemResolver.TryUpdateData(e.Item, fixtureTracker, out _);
            }


            var attachment = attachmentPool.Get();
            attachment.item = e.Item;
            attachment.currentRegion = (e.TargetPosition.LayerId, z);
            aetherBody.Tag = attachment;
        }

        void RemoveBody(ItemPositionChangedEvent<TItemId> e)
        {
            Remove(e.Item);
        }

        public void Remove(TItemId item)
        {
            if (!itemResolver.TryQueryData<AetherBodyTracker>(item, out var tracker))
            {
                return;
            }

            if (!tracker.TryGetBody(out var body))
            {
                return;
            }

            if (body.Tag is BodyAttachment ba)
            {
                ba.currentRegion = Optional.Empty();
            }

            body.World.Remove(body);
        }

        bool TryCreateShape(IPhysicsShape2D shape, Transform t, BufferList<Shape> resultCollector)
        {
            return shape switch
            {
                BoxPhysicsShape2D boxPhysicsShape2D => TryCreateBoxShape(boxPhysicsShape2D, t, resultCollector),
                PolygonPhysicsShape2D polygonPhysicsShape2D => TryCreatePolygonShape(polygonPhysicsShape2D, t, resultCollector),
                SpherePhysicsShape2D spherePhysicsShape2D => TryCreateSphereShape(spherePhysicsShape2D, t, resultCollector),
                CompoundPhysicsShape2D compoundPhysicsShape2D => TryCreateCompoundShape(compoundPhysicsShape2D, t, resultCollector),
                _ => throw new ArgumentOutOfRangeException(nameof(shape))
            };
        }

        bool TryCreateCompoundShape(CompoundPhysicsShape2D shape, Transform transform, BufferList<Shape> resultCollector)
        {
            foreach (var child in shape.Children)
            {
                var childPosition = child.Position.ToAether();
                var left = new Transform(childPosition, child.Rotation);
                var childTransform = Transform.Multiply(ref left, ref transform);
                TryCreateShape(child.Child, childTransform, resultCollector);
            }

            return false;
        }

        bool TryCreateSphereShape(SpherePhysicsShape2D shape, Transform transform, BufferList<Shape> resultCollector)
        {
            var sphere = new CircleShape(shape.Radius, shape.Weight);
            sphere.Position = Transform.Multiply(sphere.Position, ref transform);
            resultCollector.Add(sphere);
            return true;
        }

        bool TryCreatePolygonShape(PolygonPhysicsShape2D shape, Transform transform, BufferList<Shape> resultCollector)
        {
            Vertices vertices = new Vertices();
            for (var index = 0; index < shape.Points.Count; index++)
            {
                var p = shape.Points[index].ToAether();
                vertices[index] = Transform.Multiply(p, ref transform);
            }

            resultCollector.Add(new PolygonShape(vertices, shape.Weight));
            return true;
        }

        bool TryCreateBoxShape(BoxPhysicsShape2D shape, Transform transform, BufferList<Shape> resultCollector)
        {
            Vertices rectangleVertices = PolygonTools.CreateRectangle(shape.Width / 2, shape.Height / 2);
            for (var index = 0; index < rectangleVertices.Count; index++)
            {
                var p = rectangleVertices[index];
                rectangleVertices[index] = Transform.Multiply(p, ref transform);
            }

            resultCollector.Add(new PolygonShape(rectangleVertices, shape.Weight));
            return true;
        }
    }

    class BodyAttachment
    {
        public TItemId item;
        public Optional<(byte layerId, int z)> currentRegion;
    }
}