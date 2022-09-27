using MessagePack;
using RogueEntity.Core.Sensing.Common;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Receptors.Noise
{
    public class SingleLevelNoiseMap: INoiseDirectionMap
    {
        readonly SingleLevelSenseDirectionMapData<NoiseSense, NoiseSense> backend;

        [SerializationConstructor]
        public SingleLevelNoiseMap(SingleLevelSenseDirectionMapData<NoiseSense, NoiseSense> backend)
        {
            this.backend = backend;
        }

        public bool TryGetSenseData(int z, [MaybeNullWhen(false)] out IDynamicSenseDataView2D intensities)
        {
            return backend.TryGetIntensity(z, out intensities);
        }
    }
}