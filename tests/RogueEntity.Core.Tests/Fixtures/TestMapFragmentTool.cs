using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.Tests.Fixtures
{
    class TestMapFragmentTool
    {
        readonly MapBuilder builder;
        readonly IRandomGeneratorSource randomContext;
        Dictionary<byte, List<ItemDeclarationId>> availableItems;

        public TestMapFragmentTool(MapBuilder builder, IRandomGeneratorSource randomContext)
        {
            this.builder = builder;
            this.randomContext = randomContext;
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

        public void CopyToMap(IReadOnlyView2D<ItemDeclarationId[]> f,
                              Rectangle bounds,
                              EntityGridPosition origin)
        {
            if (origin == EntityGridPosition.Invalid)
            {
                throw new ArgumentException();
            }

            if (availableItems == null)
            {
                this.availableItems = PopulateItems(builder);
            }

            var randomGenerator = randomContext.RandomGenerator(origin.GetHashCode());

            var postProcessor = new ItemCreationPostProcessor(randomGenerator);
            var layers = builder.Layers;
            var itemsPerLayer = new ItemDeclarationId[layers.Count];

            foreach (var c in new RectangleContents(bounds.Width, bounds.Height))
            {
                var entry = f[c.X, c.Y];
                if (entry == null)
                {
                    continue;
                }

                Array.Clear(itemsPerLayer, 0, layers.Count);
                Array.Copy(entry, itemsPerLayer, Math.Min(entry.Length, itemsPerLayer.Length));
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
                where TEntity : struct, IEntityKey
            {
                return entityKey;
            }

            public Optional<TEntity> InstantiatePostProcess<TEntity>(ItemDeclarationId item, Position pos, IItemResolver<TEntity> itemResolver, TEntity entityKey)
                where TEntity : struct, IEntityKey
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
    }
}
