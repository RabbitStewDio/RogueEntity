using System;
using EnTTSharp.Entities;
using GoRogue;
using GoRogue.SenseMapping;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Sources.Light;

namespace RogueEntity.Core.Sensing.Discovery
{
    public class DiscoveryMapSystem
    {
        readonly Lazy<IBrightnessSource> brightnessSource;

        public DiscoveryMapSystem(Lazy<IBrightnessSource> brightnessSource)
        {
            this.brightnessSource = brightnessSource ?? throw new ArgumentNullException(nameof(brightnessSource));
        }

        void ExpandDiscoveredAreaImpl<TGameContext, TActorId, TPosition, TSense>(IEntityViewControl<TActorId> v,
                                                                                 TGameContext context,
                                                                                 TActorId k,
                                                                                 in TPosition pos,
                                                                                 in OnDemandDiscoveryMapData onDemandDiscoveryData,
                                                                                 in SensoryReceptor<TSense> vision)
            where TActorId : IEntityKey
            where TSense : ISense, IEquatable<TSense>
            where TPosition : IPosition
        {
            if (!vision.IsDirty())
            {
                return;
            }
            
            if (!brightnessSource.Value.TryGetLightData(pos.GridZ, out var senseMap))
            {
                return;
            }
            
            if (!onDemandDiscoveryData.TryGetMap(pos.GridZ, out var target))
            {
                return;
            }

            var visionField = vision.SenseData as ISenseBlitterDataSource<float>;
            var (lMin, lMax, gMin) = visionField.CalculateBounds(target.Width, target.Height);

            var light = visionField.SourceData;
            var lightWidth = visionField.Width;

            for (var yOffset = 0; yOffset <= lMax.Y - lMin.Y; yOffset++)
            {
                for (var xOffset = 0; xOffset <= lMax.X - lMin.X; xOffset++)
                {
                    var c = new Coord(xOffset, yOffset);
                    var gCur = gMin + c;
                    var lCur = lMin + c;

                    if (senseMap[gCur.X, gCur.Y] <= 0)
                    {
                        continue;
                    }

                    if (!target[gCur.X, gCur.Y] && 
                        light[lCur.X + lCur.Y * lightWidth] > 0)
                    {
                        target[gCur.X, gCur.Y] = true;
                    }
                }
            }
        }

        public void ExpandDiscoveredAreaContinuous<TGameContext, TActorId>(IEntityViewControl<TActorId> v,
                                                                           TGameContext context,
                                                                           TActorId k,
                                                                           in ContinuousMapPosition pos,
                                                                           in OnDemandDiscoveryMapData onDemandDiscoveryData,
                                                                           in SensoryReceptor<VisionSense> vision)
            where TActorId : IEntityKey
        {
            ExpandDiscoveredAreaImpl(v, context, k, in pos, in onDemandDiscoveryData, in vision);
        }

        public void ExpandDiscoveredAreaGrid<TGameContext, TActorId>(IEntityViewControl<TActorId> v,
                                                                     TGameContext context,
                                                                     TActorId k,
                                                                     in EntityGridPosition pos,
                                                                     in OnDemandDiscoveryMapData onDemandDiscoveryData,
                                                                     in SensoryReceptor<VisionSense> vision)
            where TActorId : IEntityKey
        {
            ExpandDiscoveredAreaImpl(v, context, k, in pos, in onDemandDiscoveryData, in vision);
        }
    }
}