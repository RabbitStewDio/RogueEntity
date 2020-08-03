using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using GoRogue;
using GoRogue.MapViews;
using GoRogue.SenseMapping;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Sensing.Vision
{
    public class SmartSenseSource : ISenseSource
    {
        static readonly ILogger Logger = SLog.ForContext<SmartSenseSource>();

        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                if (Math.Abs(radius - value) < 0.005f)
                {
                    return;
                }

                radius = value;
                Dirty = true;
            }
        }

        public DistanceCalculation DistanceCalculation
        {
            get
            {
                return distanceCalculation;
            }
            set
            {
                if (distanceCalculation == value)
                {
                    return;
                }

                distanceCalculation = value;
                Dirty = true;
            }
        }

        public float Angle
        {
            get
            {
                return angle;
            }
            set
            {
                if (Math.Abs(angle - value) < 0.005f)
                {
                    return;
                }

                angle = value;
                Dirty = true;
            }
        }

        public float Span
        {
            get
            {
                return span;
            }
            set
            {
                if (Math.Abs(span - value) < 0.005f)
                {
                    return;
                }

                span = value;
                Dirty = true;
            }
        }

        public float Intensity
        {
            get
            {
                return intensity;
            }
            set
            {
                if (Math.Abs(intensity - value) < 0.005f)
                {
                    return;
                }

                intensity = value;
                Dirty = true;
            }
        }

        public SourceType Type
        {
            get
            {
                return type;
            }
            set
            {
                if (type == value)
                {
                    return;
                }

                type = value;
                Dirty = true;
            }
        }

        public Coord Position => backend.Position;
        float radius;
        DistanceCalculation distanceCalculation;
        float angle;
        float span;
        float intensity;
        SourceType type;
        readonly SenseSource backend;
        
        public bool LastSeenAsActive { get; set; }
        public int ModificationCounter { get; private set; }

        public SmartSenseSource(SourceType type,
                                float radius,
                                DistanceCalculation distanceCalc,
                                float intensity = 1,
                                float angle = 0,
                                float span = 360)
        {
            backend = new SenseSource(type, 0, 0, radius, distanceCalc, angle, span, intensity);

            Type = type;
            Radius = radius;
            DistanceCalculation = distanceCalc;
            Angle = angle;
            Span = span;
            Intensity = intensity;
            Dirty = true;
        }

        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void Reset(SourceType type,
                          float radius,
                          DistanceCalculation distance,
                          float intensity = 1,
                          float angle = 0,
                          float span = 360)
        {
            Type = type;
            Radius = radius;
            DistanceCalculation = distance;
            Angle = angle;
            Span = span;
            Intensity = intensity;
        }

        public void Calculate<TResistanceMap>(TResistanceMap resistanceMap) where TResistanceMap: IMapView<float>
        {
            if (!Dirty)
            {
                Logger.Verbose("Skipped calculation as light at {Position} is not dirty.", Position);
                return;
            }


            if (Enabled)
            {
                Logger.Verbose("Recalculate light at {Position}.", Position);
                backend.Position = Position;
                backend.Type = type;
                backend.DistanceCalculation = distanceCalculation;
                backend.Angle = angle;
                backend.Span = span;
                backend.Intensity = intensity;
                backend.Radius = radius;
                backend.Calculate(resistanceMap);
            }
            else
            {
                Logger.Verbose("Clear light at {Position}.", Position);
                backend.Clear();
            }

            // a quick and dirty way of tracking whether changes have happened.
            ModificationCounter += 1;
            Dirty = false;
        }

        public void UpdateStrength(float radius, float intensity)
        {
            Dirty |= Math.Abs(Radius - radius) > 0.05f ||
                     Math.Abs(intensity - Intensity) > 0.05F;

            this.Radius = radius;
            this.Intensity = intensity;
        }

        public bool Dirty { get; private set; }

        public void MarkDirty()
        {
            Dirty = true;
        }

        public bool Contains(int x, int y)
        {
            return this.DistanceCalculation.Calculate(backend.Position, new Coord(x, y)) <= Radius;
        }

        public bool TryQuery(int x, int y, out float value)
        {
            if (backend.TryGetLocalCoordinate(new Coord(x, y), out var tp))
            {
                value = backend[tp.X, tp.Y];
                return true;
            }

            value = default;
            return false;
        }

        public void UpdatePosition(int x, int y)
        {
            var p = new Coord(x, y);
            if (backend.Position != p)
            {
                Dirty = true;
                backend.Position = new Coord(x, y);
            }
        }

        public bool Enabled
        {
            get { return backend.Enabled; }
            set
            {
                if (backend.Enabled == value)
                {
                    return;
                }

                backend.Enabled = value;
                Dirty = true;
            }
        }

        public (Coord lightMin, Coord lightMax, Coord globalMin) CalculateBounds(int width, int height)
        {
            return backend.CalculateBounds(width, height);
        }

        public float[] SourceData => backend.SourceData;
        public int Width => backend.Width;

        public void CopyFrom(SmartSenseSource senseSource)
        {
            Type = senseSource.Type;
            DistanceCalculation = senseSource.DistanceCalculation;
            Angle = senseSource.Angle;
            Span = senseSource.Span;
            Intensity = senseSource.Intensity;
            Radius = senseSource.Radius;
            Enabled = senseSource.Enabled;
        }

        public string PrintDiagnostics()
        {
            var w = backend.Width;
            var sb = new StringBuilder();

            var lc = 0;
            for (var i = 0; i < SourceData.Length; i+= 1)
            {
                sb.Append($"{SourceData[i] * 100,5:0.0} ");
                lc += 1;
                if (lc == w)
                {
                    lc = 0;
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}