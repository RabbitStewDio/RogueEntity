using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    public interface IHeatPhysicsConfiguration
    {
        ISensePhysics HeatPhysics { get; }

        /// <summary>
        ///   Defines the normal temperature of the world. This is the
        ///   neutral temperature that exists if nothing else is defined.
        ///
        ///   All temperature values in the game will attempt to normalize
        ///   towards this value.
        ///
        ///   This value can change over time. 
        /// </summary>
        public Temperature GetEnvironmentTemperature(int z);

        ISensePropagationAlgorithm CreateHeatPropagationAlgorithm();
    }
}