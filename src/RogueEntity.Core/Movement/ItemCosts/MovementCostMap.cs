using System.Collections.Generic;
using RogueEntity.Core.Infrastructure.Positioning.Grid;
using RogueEntity.Core.Sensing.Maps;
using RogueEntity.Core.Utils.MapChunks;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Movement.ItemCosts
{
    public class MovementCostMap<TGameContext> : IReadOnlyMapData<MovementCostProperties>
    {
        readonly Dictionary<MapLayer, IMovementCostDataProcessor<TGameContext>> dependencies;
        readonly CombinedBlitterResultProcessor<TGameContext, IMovementCostDataProcessor<TGameContext>> combiner;
        readonly byte[] data;

        public int Height { get; }
        public int Width { get; }

        readonly int spanSizeX;
        readonly int spanSizeY;

        public MovementCostMap(IAddByteBlitter blitter, int width, int height)
        {
            (spanSizeX, spanSizeY) = ChunkProcessor.ComputeSpanSize(blitter, width, height);

            this.dependencies = new Dictionary<MapLayer, IMovementCostDataProcessor<TGameContext>>();
            data = new byte[width * height * 4];
            combiner = new CombinedBlitterResultProcessor<TGameContext, IMovementCostDataProcessor<TGameContext>>(width, height, spanSizeX, spanSizeY, data, blitter);

            Height = height;
            Width = width;
        }

        public void AddProcess(MapLayer layer, IMovementCostDataProcessor<TGameContext> p)
        {
            dependencies.Add(layer, p);
            combiner.Add(p);
        }

        public MovementCostProperties this[int x, int y]
        {
            get
            {
                var offset = (x + y * Width) * 4;
                return new MovementCostProperties(
                    new MovementCost(data[offset]),
                    new MovementCost(data[offset + 1]),
                    new MovementCost(data[offset + 2]),
                    new MovementCost(data[offset + 3])
                );
            }
        }

        public void MarkDirty(MapLayer l, EntityGridPosition pos)
        {
            if (l == MapLayer.Indeterminate)
            {
                foreach (var d in dependencies.Values)
                {
                    d.MarkDirty(pos.GridX, pos.GridY);
                }
            }
            else if (dependencies.TryGetValue(l, out var d))
            {
                d.MarkDirty(pos.GridX, pos.GridY);
            }
        }

        public void ResetDirtyFlags()
        {
            foreach (var d in dependencies.Values)
            {
                d.ResetDirtyFlags();
            }
        }

        public void Process(TGameContext c)
        {
            foreach (var d in dependencies.Values)
            {
                d.Process(c);
            }

            combiner.Process(c);
        }
    }
}