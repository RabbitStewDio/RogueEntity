using System;
using GoRogue;
using GoRogue.SenseMapping;

namespace RogueEntity.Core.Sensing.Vision
{
    public readonly struct VisibilityDetector<TGameContext, TActorId> 
    {
        public readonly float SenseRadius;
        public readonly float SenseStrength;
        public readonly VisibilityFunctions.CanSenseAt<TGameContext, TActorId> SenseStrengthHandler;
        public readonly VisibilityFunctions.CanSenseAt<TGameContext, TActorId> SenseBlockHandler;
        public readonly SmartSenseSource VisionField;
        public readonly int VisionFieldChangeTracker;

        public VisibilityDetector(float radius, 
                                  float senseStrength,
                                  VisibilityFunctions.CanSenseAt<TGameContext, TActorId> senseBlockHandler,
                                  VisibilityFunctions.CanSenseAt<TGameContext, TActorId> senseStrengthHandler)
        {
            SenseStrength = senseStrength;
            SenseRadius = radius;
            SenseBlockHandler = senseBlockHandler ?? throw new ArgumentNullException();
            SenseStrengthHandler = senseStrengthHandler ?? throw new ArgumentNullException();
            VisionField = new SmartSenseSource(SourceType.SHADOW, SenseRadius, DistanceCalculation.EUCLIDEAN);
            VisionFieldChangeTracker = VisionField.ModificationCounter;
        }

        VisibilityDetector(float radius, float senseStrength,
                                  VisibilityFunctions.CanSenseAt<TGameContext, TActorId> senseBlockHandler,
                                  VisibilityFunctions.CanSenseAt<TGameContext, TActorId> senseStrengthHandler,
                                  SmartSenseSource visionField)
        {
            SenseStrength = senseStrength;
            SenseRadius = radius;
            SenseBlockHandler = senseBlockHandler ?? throw new ArgumentNullException();
            SenseStrengthHandler = senseStrengthHandler ?? throw new ArgumentNullException();
            VisionField = visionField;
            VisionFieldChangeTracker = visionField.ModificationCounter;
        }

        public bool IsVisibleAt(int x, int y, out float lightStrength)
        {
            if (VisionField == null || VisionField.Enabled == false)
            {
                lightStrength = 0;
                return false;
            }

            return VisionField.TryQuery(x, y, out lightStrength);
        }

        public VisibilityDetector<TGameContext, TActorId> WithClearedChangeTracker()
        {
            return new VisibilityDetector<TGameContext, TActorId>(SenseRadius, SenseStrength, SenseBlockHandler, SenseStrengthHandler, VisionField);
        }
    }
}