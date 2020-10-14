using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoRogue;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Common
{
    public class SenseDataMapServices
    {
        readonly List<ProcessData> parameterBuffer;
        static readonly Action<ProcessData> ProcessDataDelegate = ProcessTile;
        public SenseDataMapServices()
        {
            parameterBuffer = new List<ProcessData>();
        }

        readonly struct ProcessData
        {
            public readonly List<(Position2D pos, SenseSourceData sense)> senses;
            public readonly ISenseDataBlitter blitter;
            public readonly Rectangle bounds;
            public readonly BoundedDataView<float> tile;
            public readonly BoundedDataView<byte> dir;

            public ProcessData(List<(Position2D, SenseSourceData)> senses,
                               ISenseDataBlitter blitter,
                               Rectangle bounds,
                               BoundedDataView<float> tile,
                               BoundedDataView<byte> dir)
            {
                this.senses = senses;
                this.blitter = blitter;
                this.bounds = bounds;
                this.tile = tile;
                this.dir = dir;
            }
        }

        void QuerySenseDataTiles(SenseDataMap m,
                                 ISenseDataBlitter blitter,
                                 List<(Position2D, SenseSourceData)> senses)
        {
            var affectedBounds = GetSenseBounds(senses);
            var tsX = m.TileSizeX;
            var tsY = m.TileSizeY;

            for (var ty = affectedBounds.Y; ty <= affectedBounds.MaxExtentY; ty += tsY)
            for (var tx = affectedBounds.X; tx <= affectedBounds.MaxExtentX; tx += tsX)
            {
                m.FetchRawData(tx, tx, out var tile, out var dir);
                var bounds = tile.Bounds.GetIntersection(affectedBounds);
                parameterBuffer.Add(new ProcessData(senses, blitter, bounds, tile, dir));
            }
        }

        static void ProcessTile(ProcessData data)
        {
            var brightnessData = data.tile;
            var directionsData = data.dir;

            brightnessData.Clear(data.bounds);
            directionsData.Clear(data.bounds);

            foreach (var s in data.senses)
            {
                var senseBounds = LightToRectangle(s.pos, s.sense);
                if (!senseBounds.Intersects(data.bounds))
                {
                    continue;
                }

                var sense = s.sense;
                var pos = s.pos;
                data.blitter.Blit(data.bounds.GetIntersection(senseBounds), pos, sense, brightnessData, directionsData);
            }
        }

        public void ProcessSenseSources(SenseDataMap m,
                                        ISenseDataBlitter blitter,
                                        List<(Position2D pos, SenseSourceData sense)> senses)
        {
            if (senses.Count == 0)
            {
                return;
            }

            parameterBuffer.Clear();
            QuerySenseDataTiles(m, blitter, senses);
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

        static Rectangle LightToRectangle(Position2D pos, SenseSourceData sd) => new Rectangle(new Coord(pos.X, pos.Y), sd.Radius, sd.Radius);
    }
}