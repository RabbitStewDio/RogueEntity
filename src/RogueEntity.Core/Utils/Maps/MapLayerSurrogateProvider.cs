using System;
using EnTTSharp.Serialization.Xml;

namespace RogueEntity.Core.Utils.Maps
{
    public class MapLayerSurrogateProvider : SerializationSurrogateProviderBase<MapLayer, SurrogateContainer<byte>>
    {
        readonly MapLayerLookupDelegate registry;

        public MapLayerSurrogateProvider(MapLayerRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            this.registry = registry.TryGetValue;
        }

        public MapLayerSurrogateProvider(MapLayerLookupDelegate registry)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public override MapLayer GetDeserializedObject(SurrogateContainer<byte> surrogate)
        {
            var id = surrogate.Content;
            if (registry(id, out var l))
            {
                return l;
            }

            throw new SurrogateResolverException($"Unable to resolve map layer with id {id}");
        }

        public override SurrogateContainer<byte> GetObjectToSerialize(MapLayer obj)
        {
            return new SurrogateContainer<byte>(obj.LayerId);
        }
    }
}