using System;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public class NoisePhysicsConfiguration: INoisePhysicsConfiguration, IDisposable
    {
        readonly FloodFillWorkingDataSource workingDataSource;

        public NoisePhysicsConfiguration(ISensePhysics noisePhysics, FloodFillWorkingDataSource? workingDataSource = null)
        {
            this.NoisePhysics = noisePhysics ?? throw new ArgumentNullException(nameof(noisePhysics));
            this.workingDataSource = workingDataSource ?? new FloodFillWorkingDataSource();
        }

        public ISensePhysics NoisePhysics { get; }
        
        public ISensePropagationAlgorithm CreateNoisePropagationAlgorithm()
        {
            return new FloodFillPropagationAlgorithm(NoisePhysics, workingDataSource);
        }

        public void Dispose()
        {
            workingDataSource.Dispose();
        }
    }
}