using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    public interface ILightPhysicsConfiguration
    {
        ISensePhysics LightPhysics { get; }
    }
}