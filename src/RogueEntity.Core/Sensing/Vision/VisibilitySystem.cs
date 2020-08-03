using System;
using EnttSharp.Entities;
using GoRogue.MapViews;
using RogueEntity.Core.Infrastructure.Positioning;
using RogueEntity.Core.Infrastructure.Positioning.Grid;

namespace RogueEntity.Core.Sensing.Vision
{
    public static class VisibilitySystem<TGameContext, TActorId> 
        where TActorId : IEntityKey
        where TGameContext: IMapBoundsContext, ISenseContextProvider
    {
        public static void DoUpdateLocalSenseMap(IEntityViewControl<TActorId> v,
                                                 TGameContext context,
                                                 TActorId key,
                                                 in EntityGridPosition pos,
                                                 in VisibilityDetector<TGameContext, TActorId> d)
        {
            if (pos == EntityGridPosition.Invalid)
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