using EnTTSharp.Entities;
using GoRogue;
using GoRogue.SenseMapping;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Vision;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing
{
    public static class DiscoveryMapSystem
    {
        public static void ExpandDiscoveredArea<TGameContext, TActorId>(IEntityViewControl<TActorId> v,
                                                                        TGameContext context,
                                                                        TActorId k,
                                                                        in ContinuousMapPosition pos,
                                                                        in DiscoveryMapData discoveryData,
                                                                        in VisibilityDetector<TGameContext, TActorId> vision)
            where TActorId : IEntityKey
            where TGameContext : ISenseContextProvider
        {
            var grid = EntityGridPosition.OfRaw(pos.LayerId, pos.GridX, pos.GridY, pos.GridZ);
            ExpandDiscoveredArea(v, context, k, in grid, in discoveryData, in vision);
        }

        public static void ExpandDiscoveredArea<TGameContext, TActorId>(IEntityViewControl<TActorId> v,
                                                                        TGameContext context,
                                                                        TActorId k,
                                                                        in EntityGridPosition pos, 
                                                                        in DiscoveryMapData discoveryData, 
                                                                        in VisibilityDetector<TGameContext, TActorId> vision) 
            where TActorId : IEntityKey
            where TGameContext: ISenseContextProvider
        {
            if (vision.VisionFieldChangeTracker == vision.VisionField.ModificationCounter)
            {
                return;
            }

            var senseMap = context.SenseContext.BrightnessMap;
            var target = discoveryData.Map;

            var visionField = vision.VisionField as IBlitterDataSource<float>;
            var (lMin, lMax, gMin) = visionField.CalculateBounds(target.Width, target.Height);

            var light = visionField.SourceData;
            var lightWidth = visionField.Width;
            var z = pos.GridZ;

            for (var yOffset = 0; yOffset <= lMax.Y - lMin.Y; yOffset++)
            {
                for (var xOffset = 0; xOffset <= lMax.X - lMin.X; xOffset++)
                {
                    var c = new Coord(xOffset, yOffset);
                    var gCur = gMin + c;
                    var lCur = lMin + c;

                    if (senseMap[gCur.X, gCur.Y, z] == Percentage.Empty)
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

        public static void RecordModifications<TGameContext, TActorId>(IEntityViewControl<TActorId> v,
                                                                       TGameContext context,
                                                                       TActorId k,
                                                                       in VisibilityDetector<TGameContext, TActorId> vision) 
            where TActorId : IEntityKey
        {
            if (vision.VisionFieldChangeTracker != vision.VisionField.ModificationCounter)
            {
                v.WriteBack(k, vision.WithClearedChangeTracker());
            }
        }
    }
}