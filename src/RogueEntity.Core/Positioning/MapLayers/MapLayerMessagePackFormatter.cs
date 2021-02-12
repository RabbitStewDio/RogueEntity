using System;
using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Core.Positioning.MapLayers
{
    public class MapLayerMessagePackFormatter : IMessagePackFormatter<MapLayer>
    {
        readonly MapLayerLookupDelegate registry;

        public MapLayerMessagePackFormatter(IMapLayerRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            this.registry = registry.TryGetValue;
        }

        public MapLayerMessagePackFormatter(MapLayerLookupDelegate registry)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public void Serialize(ref MessagePackWriter writer, MapLayer value, MessagePackSerializerOptions options)
        {
            writer.Write(value.LayerId);
        }

        public MapLayer Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var id = reader.ReadByte();
            if (registry(id, out var l))
            {
                return l;
            }

            throw new MessagePackSerializationException($"Unable to resolve map layer with id {id}");
        }
    }
}