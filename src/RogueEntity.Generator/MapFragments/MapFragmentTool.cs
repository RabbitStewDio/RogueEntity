using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using GoRogue.Util;
using RogueEntity.Core.Infrastructure.Meta.Base;
using RogueEntity.Core.Infrastructure.Meta.Items;
using RogueEntity.Core.Infrastructure.Positioning.Grid;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Movement.ItemCosts;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using RogueEntity.Generator.MapFragments;
using Serilog;

namespace ValionRL.Core.MapFragments
{
    public static class MapFragmentTool
    {
        static readonly ILogger logger = SLog.ForContext(typeof(MapFragmentTool));

        public static void CopyItemsToMap<TGameContext, TItemId>(this TGameContext context, 
                                                                 MapFragment f, 
                                                                 EntityGridPosition position,
                                                                 MapLayer layerToCopy)
            where TGameContext : IItemContext<TGameContext, TItemId>, IRandomContext, IGridMapContext<TGameContext, TItemId>
            where TItemId : IEntityKey
        {
            if (position == EntityGridPosition.Invalid)
            {
                throw new ArgumentException();
            }

            var items = context.ItemResolver.ItemRegistry.Items;
            var itemsByPrefix = new List<IItemDeclaration>();

            var width = f.MapData.Width;
            var randomGenerator = context.RandomGenerator(new ConstantRandomSeedSource(129),
                                                          position.GetHashCode());

            if (!context.TryGetGridDataFor(layerToCopy, out var itemMapContext))
            {
                return;
            }
            
            
            foreach (var c in AreaRange.Of(width, f.MapData.Height))
            {
                var entry = f.MapData[c.X, c.Y];
                if (entry == MapFragmentTagDeclaration.Empty)
                {
                    continue;
                }

                var targetX = position.GridX + c.X;
                var targetY = position.GridY + c.Y;
                if (!IsValidMapPosition(itemMapContext, targetX, targetY))
                {
                    continue;
                }

                if (PopulateMatchingGround(items, entry, itemsByPrefix))
                {
                    var index = randomGenerator.Next(0, itemsByPrefix.Count);
                    var item = itemsByPrefix[index];
                    PopulateItem(context, randomGenerator, groundData, MapLayers.Ground, targetX, targetY, position.Z, item);
                }

                if (PopulateMatchingItem(items, entry, itemsByPrefix))
                {
                    var index = randomGenerator.Next(0, itemsByPrefix.Count);
                    var item = itemsByPrefix[index];
                    PopulateItem(context, randomGenerator, itemData, MapLayers.Items, targetX, targetY, position.Z, item);
                }
                else
                {
                    PopulateItem(context, randomGenerator, itemData, MapLayers.Items, targetX, targetY, position.Z, default);
                }
            }
        }

        static void PopulateItem<TGameContext>(TGameContext context,
                                               Func<double> randomGenerator,
                                               IMapData<ItemReference> groundData,
                                               MapLayer layer,
                                               int targetX,
                                               int targetY,
                                               int z,
                                               IItem<TGameContext> item)
            where TGameContext : IMapContext, IGameContext<TGameContext>, IGameContext
        {
            var existing = groundData[targetX, targetY];
            if (!existing.Empty)
            {
                if (existing.ReferencedItem)
                {
                    context.ItemResolver.TryUpdateData(existing, context, EntityMapPosition.Invalid, out _);
                    context.ItemResolver.Destroy(existing);
                }

                context.ItemResolver.Destroy(existing);
            }

            groundData[targetX, targetY] = default;

            if (item == null)
            {
                return;
            }

            var itemRef = context.Instantiate(item)
                                 .WithRandomizedProperties(randomGenerator)
                                 .ToItemReference;
            context.Rules.PlaceItem(context, itemRef, EntityMapPosition.Of(layer, targetX, targetY, z), true);
        }

        public static void CopyActorsToMap<TGameContext>(this TGameContext context, MapFragment f, EntityMapPosition position)
            where TGameContext : IMapContext, IGameContext<TGameContext>, IGameContext
        {
            if (position == EntityMapPosition.Invalid)
            {
                throw new ArgumentException();
            }

            var actors = context.ActorRegistry.Actors;
            var actorsByIndex = new List<IActor<TGameContext>>();

            var width = f.MapData.Width;
            var randomGenerator = context.RandomGenerator(default, position.GetHashCode());

            foreach (var c in AreaRange.Of(width, f.MapData.Height))
            {
                var entry = f.MapData[c.X, c.Y];

                var targetX = position.X + c.X;
                var targetY = position.Y + c.Y;
                if (!IsValidMapPosition(context.Map, targetX, targetY))
                {
                    continue;
                }

                if (!PopulateMatchingActor(actors, entry, actorsByIndex))
                {
                    continue;
                }

                var index = randomGenerator.Next(0, actorsByIndex.Count);
                var actor = actorsByIndex[index];

                if (actor.TryQuery(out IMovementTrait<TGameContext> mt))
                {
                    var emp = EntityMapPosition.Of(MapLayers.Actor, targetX, targetY, position.Z);
                    if (mt.BaseMovementCost == MovementCost.Blocked ||
                        !mt.CalculateVariableCellCost(context, emp, out _))
                    {
                        continue;
                    }
                }

                context.PlaceActor(actor, targetX, targetY, position.Z);
            }
        }

        static bool IsValidMapPosition<TGameContext, TItemId>(IGridMapDataContext<TGameContext, TItemId> map, int targetX, int targetY)
        {
            if (targetX < 0 || targetY < 0)
            {
                return false;
            }

            if (targetX >= map.Width)
            {
                return false;
            }

            if (targetY >= map.Height)
            {
                return false;
            }

            return true;
        }

        static bool PopulateMatchingItem<TEntity>(ReadOnlyListWrapper<TEntity> itemDatabase,
                                                  MapFragmentTagDeclaration tag,
                                                  List<TEntity> itemResult)
            where TEntity : IWorldEntity
        {
            itemResult.Clear();
            if (string.IsNullOrEmpty(tag.ItemTag))
            {
                return false;
            }

            foreach (var i in itemDatabase)
            {
                if (i.Id.MatchGlob(tag.ItemTag) ||
                    i.Id.StartsWith(tag.ItemTag))
                {
                    itemResult.Add(i);
                }
            }

            if (itemResult.Count == 0)
            {
                logger.Warning("Unable to resolve any item for prefix '{Pattern}'", tag.ItemTag);
            }

            return itemResult.Count > 0;
        }

        static bool PopulateMatchingGround<TEntity>(ReadOnlyListWrapper<TEntity> itemDatabase,
                                                    MapFragmentTagDeclaration tag,
                                                    List<TEntity> itemResult)
            where TEntity : IWorldEntity
        {
            itemResult.Clear();
            if (string.IsNullOrEmpty(tag.GroundTag))
            {
                return false;
            }

            foreach (var i in itemDatabase)
            {
                if (i.Id.MatchGlob(tag.GroundTag) ||
                    i.Id.StartsWith(tag.GroundTag))
                {
                    itemResult.Add(i);
                }
            }

            if (itemResult.Count == 0)
            {
                logger.Warning("Unable to resolve any item for prefix '{Pattern}'", tag.GroundTag);
            }

            return itemResult.Count > 0;
        }

        static bool PopulateMatchingActor<TEntity>(ReadOnlyListWrapper<TEntity> itemDatabase,
                                                   MapFragmentTagDeclaration tag,
                                                   List<TEntity> itemResult)
            where TEntity : IWorldEntity
        {
            itemResult.Clear();
            if (string.IsNullOrEmpty(tag.ActorTag))
            {
                return false;
            }

            foreach (var i in itemDatabase)
            {
                if (i.Id.MatchGlob(tag.ActorTag) ||
                    i.Id.StartsWith(tag.ActorTag))
                {
                    itemResult.Add(i);
                }
            }

            if (itemResult.Count == 0)
            {
                logger.Warning("Unable to resolve any actor for prefix '{Pattern}'", tag.ActorTag);
            }

            return itemResult.Count > 0;
        }
    }
}