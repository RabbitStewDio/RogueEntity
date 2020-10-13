namespace RogueEntity.Core.Sensing
{
    public interface ISense
    {
        /// <summary>
        ///    Defines how far ahead an actor is able to detect signals. This directly affects
        ///    the area an actor has to scan for sensory input. Keep it sane please, an infinite
        ///    sense radius will consume infinite resources. 
        /// </summary>
        float SenseRadius { get; }
        
        /// <summary>
        ///    Defines the initial sense strength an actor has. This strength is modified by
        ///    distance and affects detection of far away objects.  
        /// </summary>
        float SenseStrength { get; }
    }
}