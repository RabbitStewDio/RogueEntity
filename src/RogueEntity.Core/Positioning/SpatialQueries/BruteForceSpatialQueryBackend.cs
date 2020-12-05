using EnTTSharp.Entities;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Positioning.SpatialQueries
{
    public class BruteForceSpatialQueryBackend<TItemId> : ISpatialQuery<TItemId>
        where TItemId : IEntityKey
    {
        readonly EntityRegistry<TItemId> registry;

        public BruteForceSpatialQueryBackend(EntityRegistry<TItemId> registry)
        {
            this.registry = registry;
        }

        public void Query2D<TComponent>(ReceiveSpatialQueryResult<TItemId, TComponent> receiver,
                                        in Position pos,
                                        float distance = 1,
                                        DistanceCalculation d = DistanceCalculation.Euclid)
        {
            if (TryGetView<EntityGridPosition, TComponent>(out var gridView))
            {
                gridView.ApplyWithContext(new Context<TComponent>(receiver, pos, d), AddResult2D);
            }

            if (TryGetView<ContinuousMapPosition, TComponent>(out var conView))
            {
                conView.ApplyWithContext(new Context<TComponent>(receiver, pos, d), AddResult2D);
            }
        }

        public void Query3D<TComponent>(ReceiveSpatialQueryResult<TItemId, TComponent> receiver,
                                        in Position pos,
                                        float distance = 1,
                                        DistanceCalculation d = DistanceCalculation.Euclid)
        {
            if (TryGetView<EntityGridPosition, TComponent>(out var gridView))
            {
                gridView.ApplyWithContext(new Context<TComponent>(receiver, pos, d), AddResult3D);
            }

            if (TryGetView<ContinuousMapPosition, TComponent>(out var conView))
            {
                conView.ApplyWithContext(new Context<TComponent>(receiver, pos, d), AddResult3D);
            }
        }

        static void AddResult2D<TPosition, TComponent>(IEntityViewControl<TItemId> v,
                                                       Context<TComponent> context,
                                                       TItemId k,
                                                       in TPosition pos,
                                                       in TComponent c)
            where TPosition : IPosition<TPosition>
        {
            if (pos.GridX != context.Origin.GridX)
            {
                return;
            }

            var localPos = Position.From(pos);
            var dist = context.DistanceCalculator.Calculate(localPos, context.Origin);
            context.Receiver(new SpatialQueryResult<TItemId, TComponent>(k, Position.From(pos), c, (float)dist));
        }

        static void AddResult3D<TPosition, TComponent>(IEntityViewControl<TItemId> v,
                                                       Context<TComponent> context,
                                                       TItemId k,
                                                       in TPosition pos,
                                                       in TComponent c)
            where TPosition : IPosition<TPosition>
        {
            var localPos = Position.From(pos);
            var dist = context.DistanceCalculator.Calculate(localPos, context.Origin);
            context.Receiver(new SpatialQueryResult<TItemId, TComponent>(k, Position.From(pos), c, (float)dist));
        }

        bool TryGetView<TPosition, TComponent>(out IEntityView<TItemId, TPosition, TComponent> v)
        {
            if (this.registry.IsManaged<TPosition>() &&
                this.registry.IsManaged<TComponent>())
            {
                v = this.registry.View<TPosition, TComponent>();
                return true;
            }

            v = default;
            return false;
        }

        readonly struct Context<TComponent>
        {
            public readonly ReceiveSpatialQueryResult<TItemId, TComponent> Receiver;
            public readonly Position Origin;
            public readonly DistanceCalculation DistanceCalculator;

            public Context(ReceiveSpatialQueryResult<TItemId, TComponent> receiver, in Position origin, DistanceCalculation distanceCalculator)
            {
                Receiver = receiver;
                Origin = origin;
                DistanceCalculator = distanceCalculator;
            }
        }
    }
}