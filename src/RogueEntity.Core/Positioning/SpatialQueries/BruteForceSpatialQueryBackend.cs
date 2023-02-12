using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using System;

namespace RogueEntity.Core.Positioning.SpatialQueries
{
    public class BruteForceSpatialQueryBackend<TItemId, TComponent> : 
        SpatialQueryBackendBase<TItemId, TComponent>
        where TItemId : struct, IEntityKey
    {
        public BruteForceSpatialQueryBackend(EntityRegistry<TItemId> registry) : base(registry)
        {
        }

        protected override ICachedEntry GetEntryFactory<TPosition>(Type arg)
        {
            return new CachedEntry<TPosition>(base.Registry);
        }

        class CachedEntry<TPosition> : CachedEntryBase<TPosition>
            where TPosition : struct, IPosition<TPosition>
        {
            public CachedEntry(EntityRegistry<TItemId> registry): base(registry)
            {
            }

            public override void InvokeSphere(BufferList<SpatialQueryResult<TItemId, TComponent>> receiver,
                                              in Position pos,
                                              float distance = 1,
                                              DistanceCalculation d = DistanceCalculation.Euclid)
            {
                view?.ApplyWithContext(new SphereContext(receiver, pos, distance, d), addSphereResult);
            }

            public override void InvokeBox(BufferList<SpatialQueryResult<TItemId, TComponent>> receiver,
                                           in Rectangle3D bounds)
            {
                view?.ApplyWithContext(new BoxContext(receiver, bounds), addBoxResult);
            }
        }
    }
}