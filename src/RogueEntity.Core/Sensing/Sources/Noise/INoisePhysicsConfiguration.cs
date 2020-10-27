namespace RogueEntity.Core.Sensing.Common.Physics
{
    public interface INoisePhysicsConfiguration
    {
        ISensePhysics NoisePhysics { get; }
        ISensePropagationAlgorithm CreateNoisePropagationAlgorithm();
    }
}