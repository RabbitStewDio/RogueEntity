using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Common
{
    public class DirectionalSenseDataMapServices
    {
        static readonly Action<ProcessData> ProcessDataDelegate = ProcessTile;
        
        readonly int zLevel;
        readonly List<ProcessData> parameterBuffer;
        readonly Optional<ISenseStateCacheView> senseCache;
        List<Rectangle> partitionsBuffer;


        public DirectionalSenseDataMapServices(int zLevel, Optional<ISenseStateCacheView> senseCache)
        {
            this.zLevel = zLevel;
            this.senseCache = senseCache;
            this.parameterBuffer = new List<ProcessData>();
        }

        readonly struct ProcessData
        {
            public readonly List<(Position2D pos, SenseSourceData sense)> Senses;
            public readonly Position2D TargetPos;
            public readonly IDirectionalSenseBlitter Blitter;
            public readonly Rectangle Bounds;
            public readonly BoundedDataView<float> Tile;
            public readonly BoundedDataView<byte> Dir;

            public ProcessData(List<(Position2D, SenseSourceData)> senses,
                               Position2D targetPos,
                               IDirectionalSenseBlitter blitter,
                               Rectangle bounds,
                               BoundedDataView<float> tile,
                               BoundedDataView<byte> dir)
            {
                this.Senses = senses;
                this.TargetPos = targetPos;
                this.Blitter = blitter;
                this.Bounds = bounds;
                this.Tile = tile;
                this.Dir = dir;
            }
        }

        void QuerySenseDataTiles(SenseDataMap m, 
                                 Rectangle targetBounds,
                                 Position2D targetPos,
                                 IDirectionalSenseBlitter blitter,
                                 List<(Position2D, SenseSourceData)> senses)
        {
            var affectedBounds = GetSenseBounds(senses).GetIntersection(targetBounds);
            var tsX = m.TileSizeX;
            var tsY = m.TileSizeY;

            partitionsBuffer = affectedBounds.PartitionBy(m.OffsetX, m.OffsetY, m.TileSizeX, m.TileSizeY, partitionsBuffer);
            foreach (var r in partitionsBuffer)
            {
                m.FetchRawData(r.X, r.Y, out var tile, out var dir);
                var bounds = tile.Bounds.GetIntersection(affectedBounds);
                parameterBuffer.Add(new ProcessData(senses, targetPos, blitter, bounds, tile, dir));
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
                data.Blitter.Blit(data.Bounds.GetIntersection(senseBounds), pos, data.TargetPos, sense, brightnessData, directionsData);
            }
        }

        public void ProcessSenseSources(SenseDataMap m,
                                        Rectangle targetBounds,
                                        Position2D targetPos,
                                        IDirectionalSenseBlitter blitter,
                                        List<(Position2D pos, SenseSourceData sense)> senses)
        {
            if (senses.Count == 0)
            {
                return;
            }

            parameterBuffer.Clear();
            QuerySenseDataTiles(m, targetBounds, targetPos, blitter, senses);
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