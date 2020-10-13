using GoRogue;
using GoRogue.SenseMapping;
using RogueEntity.Core.Sensing.Sources;

namespace RogueEntity.Core.Sensing.Receptors
{
    public class SensoryReceptor<TSense> where TSense: ISense
    {
        readonly SmartSenseSource senseData;
        int changeTracker;

        public SensoryReceptor(TSense currentSenseData, SourceType sourceType = SourceType.SHADOW)
        {
            this.senseData = new SmartSenseSource(sourceType, currentSenseData.SenseRadius, DistanceCalculation.EUCLIDEAN);
        }

        public void Configure(in TSense sense)
        {
            senseData.UpdateStrength(sense.SenseRadius, sense.SenseStrength);
        }
        
        public bool IsVisibleAt(int x, int y, out float senseStrength)
        {
            if (senseData == null || senseData.Enabled == false)
            {
                senseStrength = 0;
                return false;
            }

            return senseData.TryQuery(x, y, out senseStrength);
        }

        public bool IsDirty()
        {
            return senseData.Dirty || changeTracker != senseData.ModificationCounter;
        }

        public void MarkDirty()
        {
            senseData.MarkDirty();
        }
        public void MarkClean()
        {
            changeTracker = senseData.ModificationCounter;
        }

        public bool TryGetVisibleArea(out SenseSourceView visibleArea)
        {
            if (senseData.Enabled)
            {
                visibleArea = new SenseSourceView(senseData);
                return true;
            }

            visibleArea = default;
            return false;
        }

        public void DisableSense()
        {
            senseData.Enabled = false;
        }

        public void EnableSenseAt(int positionGridX, int positionGridY, in TSense sense)
        {
            Configure(in sense);
            senseData.UpdatePosition(positionGridX, positionGridY);
        }

        public SmartSenseSource SenseData => senseData;
    }
}