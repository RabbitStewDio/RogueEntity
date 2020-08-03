using System.Collections.Generic;
using GoRogue;
using RogueEntity.Core.Utils.MapChunks;

namespace RogueEntity.Core.Sensing.Maps
{
    public class CombinedBlitterResultProcessor<TGameContext, TChunkProcessor> : ChunkProcessor<TGameContext> 
        where TChunkProcessor: ICachableChunkProcessor<TGameContext>
    {
        readonly byte[] data;
        readonly IAddByteBlitter blitter;
        readonly List<TChunkProcessor> dependencies;

        public CombinedBlitterResultProcessor(int width, int height, int spanSizeX, int spanSizeY,
                                              byte[] data,
                                              IAddByteBlitter blitter) :
            base(width, height, spanSizeX, spanSizeY)
        {
            this.data = data;
            this.blitter = blitter;
            this.dependencies = new List<TChunkProcessor>();
        }

        public void Add(TChunkProcessor processor)
        {
            dependencies.Add(processor);
        }

        public override bool CanProcess(int x, int y)
        {
            foreach (var processor in dependencies)
            {
                if (processor.CanProcess(x, y))
                    return true;
            }

            return false;
        }

        protected override void Process(TGameContext context, int yStart, int yEnd, int xStart, int xEnd)
        {
            var r = Rectangle.WithExtents(xStart, yStart, xEnd - 1, yEnd - 1);
            blitter.Process(data, Width, dependencies, in r);
        }
    }
}