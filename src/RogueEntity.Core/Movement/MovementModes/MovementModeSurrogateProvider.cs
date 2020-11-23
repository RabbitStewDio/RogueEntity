using System.Collections.Generic;
using EnTTSharp.Serialization.Xml;

namespace RogueEntity.Core.Movement.MovementModes
{
    public class MovementModeSurrogateProvider: SerializationSurrogateProviderBase<IMovementMode, SurrogateContainer<string>>
    {
        readonly Dictionary<string, IMovementMode> modes;
        
        public MovementModeSurrogateProvider(params IMovementMode[] movementModes)
        {
            this.modes = new Dictionary<string, IMovementMode>();
            foreach (var m in movementModes)
            {
                this.modes.Add(m.GetType().Name, m);
            }
        }

        public override IMovementMode GetDeserializedObject(SurrogateContainer<string> surrogate)
        {
            var surrogateKey = surrogate.Content;
            if (surrogateKey == null)
            {
                return null;
            }
            
            if (modes.TryGetValue(surrogateKey, out var result))
            {
                return result;
            }
            
            throw new SurrogateResolverException();
        }

        public override SurrogateContainer<string> GetObjectToSerialize(IMovementMode obj)
        {
            return obj?.GetType().Name;
        }
    }
    
    
}