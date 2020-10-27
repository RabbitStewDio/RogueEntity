using System;
using JetBrains.Annotations;
using RogueEntity.Core.Sensing.Common.FloodFill;

namespace RogueEntity.Core.Sensing.Common.Physics
{
    public class NoisePhysicsConfiguration: INoisePhysicsConfiguration, IDisposable
    {
        readonly FloodFillWorkingDataSource workingDataSource;

        public NoisePhysicsConfiguration([NotNull] ISensePhysics noisePhysics, FloodFillWorkingDataSource workingDataSource = null)
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