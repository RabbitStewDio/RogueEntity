using EnTTSharp.Serialization.Xml;

namespace RogueEntity.Core.Meta.StatusEffects
{
    public class StatusFlagSurrogateProvider : SerializationSurrogateProviderBase<StatusFlag, SurrogateContainer<int>>
    {
        readonly StatusFlagRegistry flagRegistry;

        public override StatusFlag GetDeserializedObject(SurrogateContainer<int> surrogate)
        {
            var linIdx = surrogate.Content;
            if (flagRegistry.TryGet(linIdx, out var flag))
            {
                return flag;
            }

            throw new SurrogateResolverException($"Unable to find status flag for linear index {linIdx}");
        }

        public override SurrogateContainer<int> GetObjectToSerialize(StatusFlag obj)
        {
            return new SurrogateContainer<int>(obj.LinearIndex);
        }
    }
}