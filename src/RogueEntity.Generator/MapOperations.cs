using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using System;
using System.Runtime.Serialization;

namespace RogueEntity.Generator
{
    public static class MapOperations
    {
        public static MapBuilder Clear(this MapBuilder b, MapLayer l, float z, Rectangle r, IMapBuilderInstantiationLifter postProcessor = null)
        {
            foreach (var pos in r.Contents)
            {
                if (!b.Clear(Position.Of(l, pos.X, pos.Y, z), postProcessor))
                {
                    throw new MapGeneratorException();
                }
            }

            return b;
        }

        public static MapBuilder Fill(this MapBuilder b, MapLayer l, float z, Rectangle r, IMapBuilderInstantiationLifter postProcessor, params ItemDeclarationId[] items)
        {
            foreach (var pos in r.Contents)
            {
                b.InstantiateAll(Position.Of(l, pos.X, pos.Y, z), postProcessor, items);
            }

            return b;
        }

        public static MapBuilder Draw(this MapBuilder b, MapLayer l, float z, Rectangle r, IMapBuilderInstantiationLifter postProcessor, params ItemDeclarationId[] items)
        {
            foreach (var pos in r.PerimeterPositions())
            {
                b.InstantiateAll(Position.Of(l, pos.X, pos.Y, z), postProcessor, items);
            }

            return b;
        }

        public static MapBuilder InstantiateAll(this MapBuilder b, Position pos, params ItemDeclarationId[] items)
        {
            return InstantiateAll(b, pos, null, items);
        }
        
        public static MapBuilder InstantiateAll(this MapBuilder b, Position pos, IMapBuilderInstantiationLifter postProcessor, params ItemDeclarationId[] items)
        {
            var layers = b.Layers;
            var maxIterationCount = Math.Min(layers.Count, items.Length);
            for (var index = 0; index < maxIterationCount; index++)
            {
                var l = layers[index];
                var item = items[index];
                if (item.IsInvalid)
                {
                    continue;
                }
                
                if (!b.Instantiate(items[index], Position.Of(l, pos.X, pos.Y, pos.Z), postProcessor))
                {
                    throw new MapGeneratorException();
                }
            }

            return b;
        }
    }

    public class MapGeneratorException: ApplicationException
    {
        public MapGeneratorException()
        {
        }

        protected MapGeneratorException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MapGeneratorException(string message) : base(message)
        {
        }

        public MapGeneratorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}