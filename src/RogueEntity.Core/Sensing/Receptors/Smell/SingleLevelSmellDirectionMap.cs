using RogueEntity.Core.Sensing.Common;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Receptors.Smell
{
    public class SingleLevelSmellDirectionMap: ISmellDirectionMap
    {
        readonly SingleLevelSenseDirectionMapData<SmellSense, SmellSense> backend;

        public SingleLevelSmellDirectionMap(SingleLevelSenseDirectionMapData<SmellSense, SmellSense> backend)
        {
            this.backend = backend;
        }

        public bool TryGetSenseData(int z, [MaybeNullWhen(false)] out IDynamicSenseDataView2D intensities)
        {
            return backend.TryGetIntensity(z, out intensities);
        }
    }
}