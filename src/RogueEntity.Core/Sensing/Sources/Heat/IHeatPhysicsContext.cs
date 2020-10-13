using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Sensing.Sources
{
    public interface IHeatPhysicsContext
    {
        /// <summary>
        ///   Defines the normal temperature of the world. This is the
        ///   neutral temperature that exists if nothing else is defined.
        ///
        ///   All temperature values in the game will attempt to normalize
        ///   towards this value.
        ///
        ///   This value can change over time. 
        /// </summary>
        public Temperature EnvironmentalTemperature { get; }

    }
}