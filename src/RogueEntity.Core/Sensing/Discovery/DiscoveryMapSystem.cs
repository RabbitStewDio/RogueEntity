using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EnTTSharp.Entities;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Utils;

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
        public void ExpandDiscoveredArea<TGameContext, TActorId, TReceptorSense, TSourceSense>(IEntityViewControl<TActorId> v,
                                                                                               TGameContext context,
                                                                                               TActorId k,
                                                                                               in DiscoveryMapData map,
                                                                                               in SensoryReceptorState<TReceptorSense, TSourceSense> receptor,
                                                                                               in SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense> vision,
                                                                                               in SenseReceptorDirtyFlag<TReceptorSense, TSourceSense> unused)
            where TActorId : IEntityKey
            where TReceptorSense : ISense
            where TSourceSense : ISense
        {
            var pos = receptor.LastPosition;
            if (!receptor.SenseSource.TryGetValue(out var sense) ||
                pos.IsInvalid)
            {
                return;
            }

            var senseBounds = new Rectangle(new Position2D(pos.GridX, pos.GridY), sense.Radius, sense.Radius);

            if (!vision.TryGetIntensity(pos.GridZ, out var senseMap))
            {
                return;
            }

            if (!map.TryGetWritableMap(pos.GridZ, out var target))
            {
                return;
            }

            // var partitions = partitionBounds.Value;
            // senseBounds.PartitionBy(map.OffsetX, map.OffsetY, map.TileWidth, map.TileHeight, partitions);

            foreach (var (x, y) in senseBounds.Contents)
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