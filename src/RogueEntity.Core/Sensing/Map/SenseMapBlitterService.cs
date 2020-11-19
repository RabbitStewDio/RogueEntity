using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Map
{
    public class SenseMapBlitterService
    {
        static readonly Action<ProcessData> ProcessDataDelegate = ProcessTile;
        
        readonly List<ProcessData> parameterBuffer;
        List<Rectangle> partitionsBuffer;
        
        public SenseMapBlitterService()
        {
            this.parameterBuffer = new List<ProcessData>();
        }

        readonly struct ProcessData
        {
            public readonly List<(Position2D pos, SenseSourceData sense)> Senses;
            public readonly ISenseMapDataBlitter Blitter;
            public readonly Rectangle Bounds;
            public readonly BoundedDataView<float> Tile;
            public readonly BoundedDataView<byte> Dir;

            public ProcessData(List<(Position2D, SenseSourceData)> senses,
                               ISenseMapDataBlitter blitter,
                               Rectangle bounds,
                               BoundedDataView<float> tile,
                               BoundedDataView<byte> dir)
            {
                this.Senses = senses;
                this.Blitter = blitter;
                this.Bounds = bounds;
                this.Tile = tile;
                this.Dir = dir;
            }
        }

        void QuerySenseDataTiles(SenseDataMap m,
                                 Rectangle targetBounds, 
                                 ISenseMapDataBlitter blitter,
                                 List<(Position2D, SenseSourceData)> senses)
        {
            var affectedBounds = GetSenseBounds(senses).GetIntersection(targetBounds);
            if (affectedBounds.IsEmpty)
            {
                return;
            }

            partitionsBuffer = affectedBounds.PartitionBy(m.OffsetX, m.OffsetY, m.TileSizeX, m.TileSizeY, partitionsBuffer);
            foreach (var r in partitionsBuffer)
            {
                // var tileBounds = new Rectangle(tx, ty, tsX, tsY); 
                m.FetchRawData(r.X, r.Y, out var tile, out var dir);
                var bounds = tile.Bounds.GetIntersection(affectedBounds);
                parameterBuffer.Add(new ProcessData(senses, blitter, bounds, tile, dir));
            }
        }

        static void ProcessTile(ProcessData data)
        {
            var brightnessData = data.Tile;
            var directionsData = data.Dir;

            brightnessData.Clear(data.Bounds);
            directionsData.Clear(data.Bounds);

            foreach (var s in data.Senses)
            {
                var senseBounds = LightToRectangle(s.pos, s.sense);
                if (!senseBounds.Intersects(data.Bounds))
                {
                    continue;
                }

                var sense = s.sense;
                var pos = s.pos;
                data.Blitter.Blit(data.Bounds.GetIntersection(senseBounds), pos, sense, brightnessData, directionsData);
            }
        }

        public void ProcessSenseSources(SenseDataMap m,
                                        in Rectangle targetBounds, 
                                        ISenseMapDataBlitter blitter,
                                        List<(Position2D pos, SenseSourceData sense)> senses)
        {
            if (senses.Count == 0)
            {
                return;
            }

            parameterBuffer.Clear();
            QuerySenseDataTiles(m, targetBounds, blitter, senses);
            Parallel.ForEach(parameterBuffer, ProcessDataDelegate);
            parameterBuffer.Clear();
        }

        public static Rectangle GetSenseBounds(List<(Position2D pos, SenseSourceData sd)> senses)
        {
            var firstEntry = senses[0];
            var bounds = LightToRectangle(firstEntry.pos, firstEntry.sd);

            for (var index = 1; index < senses.Count; index++)
            {
                var sense = senses[index];
                var r = LightToRectangle(sense.pos, sense.sd);
                bounds = bounds.GetUnion(in r);
            }

            return bounds;
        }

        static Rectangle LightToRectangle(Position2D pos, SenseSourceData sd) => new Rectangle(new Position2D(pos.X, pos.Y), sd.Radius, sd.Radius);
    }
}