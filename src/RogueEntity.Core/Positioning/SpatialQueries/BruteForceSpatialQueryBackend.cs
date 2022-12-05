using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning.SpatialQueries
{
    public class BruteForceSpatialQueryBackend<TItemId> : SpatialQueryBackendBase<TItemId>, ISpatialQuery<TItemId>
        where TItemId : struct, IEntityKey
    {
        public BruteForceSpatialQueryBackend(EntityRegistry<TItemId> registry) : base(registry)
        {
        }

        protected override ICachedEntry GetEntryFactory<TPosition, TComponent>(CachedEntryKey arg)
        {
            return new CachedEntry<TPosition, TComponent>(base.Registry);
        }

        class CachedEntry<TPosition, TComponent> : CachedEntryBase<TPosition, TComponent>
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