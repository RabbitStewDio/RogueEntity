using System;
using JetBrains.Annotations;
using RogueEntity.Core.Sensing.Common.FloodFill;

namespace RogueEntity.Core.Sensing.Common.Physics
{
    public class SmellPhysicsConfiguration: ISmellPhysicsConfiguration, IDisposable
    {
        readonly FloodFillWorkingDataSource workingDataSource;

        public SmellPhysicsConfiguration([NotNull] ISensePhysics noisePhysics, FloodFillWorkingDataSource workingDataSource = null)
        {
            this.SmellPhysics = noisePhysics ?? throw new ArgumentNullException(nameof(noisePhysics));
            this.workingDataSource = workingDataSource ?? new FloodFillWorkingDataSource();
        }

        public ISensePhysics SmellPhysics { get; }
        
        public ISensePropagationAlgorithm CreateSmellPropagationAlgorithm()
        {
            return new FloodFillPropagationAlgorithm(SmellPhysics, workingDataSource);
        }

        public void Dispose()
        {
            workingDataSource.Dispose();
        }
    }
}