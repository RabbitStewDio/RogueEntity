using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EnTTSharp.Entities;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Discovery
{
    public sealed class DiscoveryMapSystem : IDisposable
    {
        readonly ThreadLocal<List<Rectangle>> partitionBounds;

        public DiscoveryMapSystem()
        {
            this.partitionBounds = new ThreadLocal<List<Rectangle>>(() => new List<Rectangle>());
        }

        public void Dispose()
        {
            partitionBounds?.Dispose();
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public void ExpandDiscoveredArea<TGameContext, TActorId, TSourceSense, TReceptorSense>(IEntityViewControl<TActorId> v,
                                                                                               TGameContext context,
                                                                                               TActorId k,
                                                                                               in DiscoveryMapData map,
                                                                                               in SensoryReceptorState<TReceptorSense> receptor,
                                                                                               in SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense> vision,
                                                                                               in SenseReceptorDirtyFlag<TReceptorSense> unused)
            where TActorId : IEntityKey
            where TReceptorSense : ISense
        {
            var pos = receptor.LastPosition;
            if (!receptor.SenseSource.TryGetValue(out var sense) ||
                pos.IsInvalid)
            {
                return;
            }

            var partitions = partitionBounds.Value;
            var senseBounds = new Rectangle(new Position2D(pos.GridX, pos.GridY), sense.Radius, sense.Radius);
            senseBounds.PartitionBy(map.OffsetX, map.OffsetY, map.TileWidth, map.TileHeight, partitions);

            if (!vision.TryGetIntensity(pos.GridZ, out var senseMap))
            {
                return;
            }

            if (!map.TryGetWritableMap(pos.GridZ, out var target))
            {
                return;
            }

            foreach (var p in partitions)
            {
                foreach (var (x, y) in p.Contents)
                {
                    if (senseMap.TryQuery(x, y, out var intensity, out _) &&
                        intensity > 0)
                    {
                        target[x, y] = true;
                    }
                }
            }
        }
    }
}