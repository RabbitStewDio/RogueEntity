using RogueEntity.Core.Sensing.Common;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Receptors.Touch
{
    public class SingleLevelTouchDirectionMap: ITouchDirectionMap
    {
        readonly SingleLevelSenseDirectionMapData<TouchSense, TouchSense> backend;

        public SingleLevelTouchDirectionMap(SingleLevelSenseDirectionMapData<TouchSense, TouchSense> backend)
        {
            this.backend = backend;
        }

        public bool TryGetSenseData(int z, [MaybeNullWhen(false)] out IDynamicSenseDataView2D intensities)
        {
            return backend.TryGetIntensity(z, out intensities);
        }

    }
}