using System;
using EnTTSharp.Entities;
using GoRogue.MapViews;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Sensing.Vision
{
    /// <summary>
    ///    Computes an actors local vision. Vision calculation is similar to having a
    ///    private lamp that that shines on every object around the actor. The result
    ///    of this calculation is a visibility mask, that marks every potentially visible
    ///    field. This can then be combined with light information to derive the actually
    ///    visible items. (Even though you may potentially see an item in perfect light,
    ///    if there is darkness you still won't see anything.)
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    /// <typeparam name="TActorId"></typeparam>
    public static class VisibilitySystem<TGameContext, TActorId>
        where TActorId : IEntityKey
        where TGameContext : IMapBoundsContext, ISenseContextProvider
    {
        public static void DoUpdateLocalSenseMapGrid(IEntityViewControl<TActorId> v,
                                                     TGameContext context,
                                                     TActorId key,
                                                     in EntityGridPosition pos,
                                                     in VisibilityDetector<TGameContext, TActorId> d)
        {
            if (pos.IsInvalid)
            {
                d.VisionField.Enabled = false;
            }
            else
            {
                d.VisionField.Enabled = true;
                d.VisionField.UpdatePosition(pos.GridX, pos.GridY);
                d.VisionField.UpdateStrength(d.SenseRadius, d.SenseStrength);
            }

            if (d.VisionField.Dirty ||
                context.SenseContext.IsDirty(pos, (int)Math.Ceiling(d.VisionField.Radius)))
            {
                d.VisionField.MarkDirty();

                var resistor = new SmartSenseResistor(context, key, pos, d.SenseBlockHandler);
                d.VisionField.Calculate(resistor);
                resistor.ProcessSenseStrengthModifier(d.VisionField, d.SenseStrengthHandler);
            }
        }

        public static void DoUpdateLocalSenseMapContinuous(IEntityViewControl<TActorId> v,
                                                           TGameContext context,
                                                           TActorId key,
                                                           in ContinuousMapPosition pos,
                                                           in VisibilityDetector<TGameContext, TActorId> d)
        {
            var gridPos = EntityGridPosition.From(pos);
            DoUpdateLocalSenseMapGrid(v, context, key, in gridPos, in d);
        }

        readonly struct SmartSenseResistor : IMapView<float>
        {
            public int Height { get; }
            public int Width { get; }
            readonly VisibilityFunctions.CanSenseAt<TGameContext, TActorId> senseAt;
            readonly TActorId self;
            readonly TGameContext context;
            readonly Position selfPosition;

            public SmartSenseResistor(TGameContext context,
                                      TActorId self,
                                      EntityGridPosition selfPosition,
                                      VisibilityFunctions.CanSenseAt<TGameContext, TActorId> senseAt)
            {
                Width = context.MapExtent.Width;
                Height = context.MapExtent.Height;

                this.selfPosition = Position.From(selfPosition);
                this.senseAt = senseAt;
                this.self = self;
                this.context = context;
            }

            public float this[int x, int y]
            {
                get
                {
                    return senseAt(self, context, in selfPosition, selfPosition.From(x, y));
                }
            }

            public void ProcessSenseStrengthModifier(SmartSenseSource s,
                                                     VisibilityFunctions.CanSenseAt<TGameContext, TActorId> senseStrength)
            {
                var globalTx = s.Position.X - (int)s.Radius;
                var globalTy = s.Position.Y - (int)s.Radius;
                var data = s.SourceData;

                var bounds = s.CalculateBounds(Width, Height);
                var (xMin, yMin) = bounds.lightMin;
                var (xMax, yMax) = bounds.lightMax;
                for (int y = yMin; y <= yMax; y += 1)
                {
                    for (int x = xMin; x <= xMax; x += 1)
                    {
                        var mapPos = selfPosition.From(globalTx + x, globalTy + y);
                        var strength = senseStrength(self, context, in selfPosition, mapPos);
                        data[x + y * s.Width] *= strength;
                    }
                }
            }
        }
    }
}