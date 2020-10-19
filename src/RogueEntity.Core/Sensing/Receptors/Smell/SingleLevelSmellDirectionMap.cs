using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Receptors.Smell
{
    public class SingleLevelSmellDirectionMap: ISmellDirectionMap
    {
        readonly SingleLevelSenseDirectionMapData<SmellSense, SmellSense> backend;

        public SingleLevelSmellDirectionMap(SingleLevelSenseDirectionMapData<SmellSense, SmellSense> backend)
        {
            this.backend = backend;
        }

        public bool TryGetSenseData(int z, out ISenseDataView intensities)
        {
            return backend.TryGetIntensity(z, out intensities);
        }
    }
}