using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using Serilog;
using System;

namespace RogueEntity.Core.MapLoading.Builder
{
    public static class MapOperations
    {
        static readonly ILogger Logger = SLog.ForContext(typeof(MapOperations)); 
        
        public static MapBuilder Clear(this MapBuilder b, MapLayer l, float z, Rectangle r, IMapBuilderInstantiationLifter postProcessor = null)
        {
            foreach (var pos in r.Contents)
            {
                var position = Position.Of(l, pos.X, pos.Y, z);
                if (!b.Clear(position, postProcessor))
                {
                    throw new MapBuilderException($"Unable to clear position {position}");
                }
            }

            return b;
        }

        public static MapBuilder Clear(this MapBuilder b, MapLayer l, Rectangle3D r, IMapBuilderInstantiationLifter postProcessor = null)
        {
            var layerSlice = r.ToLayerSlice();
            foreach (var z in r.Layers)
            {
                foreach (var pos in layerSlice.Contents)
                {
                    var position = Position.Of(l, pos.X, pos.Y, z);
                    if (!b.Clear(position, postProcessor))
                    {
                        throw new MapBuilderException($"Unable to clear position {position}");
                    }
                }
            }

            return b;
        }

        public static MapBuilder Fill(this MapBuilder b, MapLayer l, float z, Rectangle r, params ItemDeclarationId[] items)
        {
            return Fill(b, l, z, r, null, items);
        }
        
        public static MapBuilder Fill(this MapBuilder b, MapLayer l, float z, Rectangle r, IMapBuilderInstantiationLifter postProcessor, params ItemDeclarationId[] items)
        {
            foreach (var pos in r.Contents)
            {
                b.InstantiateAll(Position.Of(l, pos.X, pos.Y, z), postProcessor, items);
            }

            return b;
        }

        public static MapBuilder Draw(this MapBuilder b, MapLayer l, float z, Rectangle r, params ItemDeclarationId[] items)
        {
            return Draw(b, l, z, r, null, items);
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

                var position = Position.Of(l, pos.X, pos.Y, pos.Z);
                if (!b.Instantiate(item, position, postProcessor))
                {
                    throw new MapBuilderException($"Unable to instantiate {item.Id} at {position}");
                }

                Logger.Verbose("Instantiated {ItemId} at {Position}", item.Id, position);
            }

            return b;
        }
    }
}
