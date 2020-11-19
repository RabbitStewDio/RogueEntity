using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Receptors
{
    public class DirectionalSenseDataMapServices
    {
        static readonly Action<ProcessData> ProcessDataDelegate = ProcessTile;
        
        readonly List<ProcessData> parameterBuffer;
        List<Rectangle> partitionsBuffer;

        public DirectionalSenseDataMapServices()
        {
            this.parameterBuffer = new List<ProcessData>();
        }

        readonly struct ProcessData
        {
            public readonly List<(Position2D pos, SenseSourceData sense)> Senses;
            public readonly Position2D TargetPos;
            public readonly ISenseReceptorBlitter ReceptorBlitter;
            public readonly Rectangle Bounds;
            public readonly BoundedDataView<float> ReceptorSenseIntensity;
            public readonly BoundedDataView<byte> ReceptorSenseDirections;

            public ProcessData(List<(Position2D, SenseSourceData)> senses,
                               Position2D targetPos,
                               ISenseReceptorBlitter receptorBlitter,
                               Rectangle bounds,
                               BoundedDataView<float> receptorSenseIntensity,
                               BoundedDataView<byte> receptorSenseDirections)
            {
                this.Senses = senses;
                this.TargetPos = targetPos;
                this.ReceptorBlitter = receptorBlitter;
                this.Bounds = bounds;
                this.ReceptorSenseIntensity = receptorSenseIntensity;
                this.ReceptorSenseDirections = receptorSenseDirections;
            }
        }

        void QuerySenseDataTiles(SenseDataMap receptorSenseMap, 
                                 Rectangle targetBounds,
                                 Position2D targetPos,
                                 ISenseReceptorBlitter receptorBlitter,
                                 List<(Position2D, SenseSourceData)> senses)
        {
            var affectedBounds = GetSenseBounds(senses).GetIntersection(targetBounds);
            partitionsBuffer = affectedBounds.PartitionBy(receptorSenseMap.OffsetX, receptorSenseMap.OffsetY, receptorSenseMap.TileSizeX, receptorSenseMap.TileSizeY, partitionsBuffer);
            foreach (var r in partitionsBuffer)
            {
                receptorSenseMap.FetchRawData(r.X, r.Y, out var receptorSenseIntensity, out var receptorSenseDirection);
                var bounds = receptorSenseIntensity.Bounds.GetIntersection(affectedBounds);
                parameterBuffer.Add(new ProcessData(senses, targetPos, receptorBlitter, bounds, receptorSenseIntensity, receptorSenseDirection));
            }
        }

        static void ProcessTile(ProcessData data)
        {
            var receptorSenseIntensity = data.ReceptorSenseIntensity;
            var receptorSenseDirections = data.ReceptorSenseDirections;

            receptorSenseIntensity.Clear(data.Bounds);
            receptorSenseDirections.Clear(data.Bounds);

            foreach (var s in data.Senses)
            {
                var senseBounds = LightToRectangle(s.pos, s.sense);
                if (!senseBounds.Intersects(data.Bounds))
                {
                    continue;
                }

                var sense = s.sense;
                var pos = s.pos;
                data.ReceptorBlitter.Blit(data.Bounds.GetIntersection(senseBounds), pos, data.TargetPos, sense, receptorSenseIntensity, receptorSenseDirections);
            }
        }

        public void ProcessSenseSources(SenseDataMap receptorSenseMap,
                                        Rectangle targetBounds,
                                        Position2D targetPos,
                                        ISenseReceptorBlitter receptorBlitter,
                                        List<(Position2D pos, SenseSourceData sense)> senses)
        {
            if (senses.Count == 0)
            {
                return;
            }

            parameterBuffer.Clear();
            QuerySenseDataTiles(receptorSenseMap, targetBounds, targetPos, receptorBlitter, senses);
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