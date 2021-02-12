using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Generator.MapFragments
{
    public class MapFragmentTool
    {
        static readonly ILogger Logger = SLog.ForContext(typeof(MapFragmentTool));

        readonly MapBuilder builder;
        readonly IEntityRandomGeneratorSource randomContext;
        readonly Dictionary<byte, List<ItemDeclarationId>> availableItems;
        BufferList<ItemDeclarationId> buffer;

        public MapFragmentTool(MapBuilder builder, IEntityRandomGeneratorSource randomContext)
        {
            this.builder = builder;
            this.randomContext = randomContext;
            this.availableItems = PopulateItems(builder);
        }

        static Dictionary<byte, List<ItemDeclarationId>> PopulateItems(MapBuilder mapBuilder)
        {
            var result = new Dictionary<byte, List<ItemDeclarationId>>();
            foreach (var layer in mapBuilder.Layers)
            {
                if (mapBuilder.TryGetItemRegistry(layer, out var itemRegistryForLayer))
                {
                    result.Add(layer.LayerId, itemRegistryForLayer.Items.Select(item => item.Id).ToList());
                }
            }

            return result;
        }

        public void CopyToMap(MapFragment f,
                              EntityGridPosition origin)
        {
            if (origin == EntityGridPosition.Invalid)
            {
                throw new ArgumentException();
            }

            var randomGenerator = randomContext.RandomGenerator(new ConstantRandomSeedSource(129),
                                                                origin.GetHashCode());

            var postProcessor = new ItemCreationPostProcessor(randomGenerator);
            var layers = builder.Layers;
            var itemsPerLayer = new ItemDeclarationId[layers.Count];

            foreach (var c in AreaRange.Of(f.Size.Width, f.Size.Height))
            {
                var entry = f.MapData[c.X, c.Y];
                if (entry == MapFragmentTagDeclaration.Empty)
                {
                    continue;
                }

                for (var i = 0; i < layers.Count; i++)
                {
                    if (!entry.TryGetTag(i, out var tag) || string.IsNullOrEmpty(tag))
                    {
                        itemsPerLayer[i] = default;
                        continue;
                    }

                    buffer = PopulateMatchingItems(layers[i], tag, buffer);
                    if (buffer.Count == 0)
                    {
                        Logger.Warning("Unable to find matching items for requested tag pattern {Tag} in layer {Layer}", tag, layers[i]);
                        itemsPerLayer[i] = default;
                        continue;
                    }

                    var selectedItem = buffer[randomGenerator.Next(0, buffer.Count)];
                    itemsPerLayer[i] = selectedItem;
                }

                var targetX = origin.GridX + c.X;
                var targetY = origin.GridY + c.Y;
                builder.InstantiateAll(Position.Of(MapLayer.Indeterminate, targetX, targetY, origin.Z), postProcessor, itemsPerLayer);
            }
        }

        class ItemCreationPostProcessor : IMapBuilderInstantiationLifter
        {
            readonly IRandomGenerator randomGenerator;

            public ItemCreationPostProcessor(IRandomGenerator randomGenerator)
            {
                this.randomGenerator = randomGenerator;
            }

            public Optional<TEntity> ClearPreProcess<TEntity>(ItemDeclarationId item, Position pos, IItemResolver<TEntity> itemResolver, TEntity entityKey)
                where TEntity : IEntityKey
            {
                return entityKey;
            }

            public Optional<TEntity> InstantiatePostProcess<TEntity>(ItemDeclarationId item, Position pos, IItemResolver<TEntity> itemResolver, TEntity entityKey)
                where TEntity : IEntityKey
            {
                if (itemResolver.TryQueryData(entityKey, out StackCount stackSize))
                {
                    var stackCount = randomGenerator.Next(1, stackSize.MaximumStackSize + 1);
                    stackSize = stackSize.WithCount((ushort)stackCount);
                    if (itemResolver.TryUpdateData(entityKey, stackSize, out var changedItemRef))
                    {
                        entityKey = changedItemRef;
                    }
                }

                if (itemResolver.TryQueryData(entityKey, out ItemCharge charge))
                {
                    var chargeCount = randomGenerator.Next(1, charge.MaximumCharge + 1);
                    if (itemResolver.TryUpdateData(entityKey, charge.WithCount(chargeCount), out var changedItemRef))
                    {
                        entityKey = changedItemRef;
                    }
                }

                if (itemResolver.TryQueryData(entityKey, out Durability durability))
                {
                    var durabilityHp = randomGenerator.Next(1, durability.MaxHitPoints + 1);
                    if (itemResolver.TryUpdateData(entityKey, durability.WithHitPoints(durabilityHp), out var changedItemRef))
                    {
                        entityKey = changedItemRef;
                    }
                }

                return entityKey;
            }
        }

        BufferList<ItemDeclarationId> PopulateMatchingItems(MapLayer mapLayer,
                                                            string tag,
                                                            BufferList<ItemDeclarationId> itemResult)
        {
            itemResult = BufferList.PrepareBuffer(itemResult);
            if (string.IsNullOrEmpty(tag))
            {
                return itemResult;
            }

            if (availableItems.TryGetValue(mapLayer.LayerId, out var itemDatabase))
            {
                foreach (var i in itemDatabase)
                {
                    if (i.Id.MatchGlob(tag) ||
                        i.Id.StartsWith(tag))
                    {
                        itemResult.Add(i);
                    }
                }
            }

            return itemResult;
        }
    }
}
