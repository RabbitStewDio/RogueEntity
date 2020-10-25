using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Receptors.Touch
{
    public class SingleLevelTouchDirectionMap: ITouchDirectionMap
    {
        readonly SingleLevelSenseDirectionMapData<TouchSense, TouchSense> backend;

        public SingleLevelTouchDirectionMap(SingleLevelSenseDirectionMapData<TouchSense, TouchSense> backend)
        {
            this.backend = backend;
        }

        public bool TryGetSenseData(int z, out ISenseDataView intensities)
        {
            return backend.TryGetIntensity(z, out intensities);
        }

    }
}