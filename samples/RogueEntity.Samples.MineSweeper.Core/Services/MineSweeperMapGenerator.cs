﻿using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Samples.MineSweeper.Core.Traits;
using System;
using System.Collections.Generic;

namespace RogueEntity.Samples.MineSweeper.Core.Services
{
    public class MineSweeperMapGenerator<TItemId>
        where TItemId : struct, IEntityKey
    {
        readonly IMineSweeperGameParameterService worldGeneratorParameterService;
        readonly IEntityRandomGeneratorSource randomSource;
        readonly IItemPlacementServiceContext<TItemId> gridMap;
        readonly IMapStateController mapStateController;
        readonly IItemResolver<TItemId> itemResolver;
        readonly MapBuilder mapBuilder;

        public MineSweeperMapGenerator(IEntityRandomGeneratorSource randomSource,
                                       IItemPlacementServiceContext<TItemId> gridMap,
                                       IMapStateController mapStateController,
                                       IItemResolver<TItemId> itemResolver,
                                       MapBuilder mapBuilder,
                                       IMineSweeperGameParameterService worldGeneratorParameterSource)
        {
            this.randomSource = randomSource;
            this.gridMap = gridMap;
            this.mapStateController = mapStateController;
            this.itemResolver = itemResolver;
            this.mapBuilder = mapBuilder;
            this.worldGeneratorParameterService = worldGeneratorParameterSource;
        }

        public void Activate()
        {
            var playerData = worldGeneratorParameterService.WorldParameter;
            if (!playerData.Validate())
            {
                throw new ArgumentException();
            }

            if (!gridMap.TryGetItemPlacementService(MineSweeperMapLayers.Items, out var itemData))
            {
                throw new InvalidOperationException();
            }

            mapStateController.ResetState();

            ValidatePlayerData(playerData);

            // Define the playing field
            // which is surrounded by a wall
            mapBuilder.Draw(MineSweeperMapLayers.Items, 0,
                            new Rectangle(0, 0, playerData.PlayFieldArea.Width + 2, playerData.PlayFieldArea.Height + 2),
                            MineSweeperItemDefinitions.Wall);

            // with a random number of mines
            var minePositions = GenerateMinePositions(playerData);
            foreach (var pos in minePositions)
            {
                mapBuilder.Instantiate(MineSweeperItemDefinitions.Mine, Position.From(pos));
            }

            // and finally precompute the neighbourhood cells indicating how many mines are near by
            foreach (var (x, y) in new RectangleContents(1, 1, playerData.PlayFieldArea.Width, playerData.PlayFieldArea.Height))
            {
                if (!mapBuilder.Instantiate(MineSweeperItemDefinitions.Floor, 
                                            Position.Of(MineSweeperMapLayers.Items, x, y)))
                {
                    // tile is already occupied.
                    continue;
                }
                
                var mines = CountAdjacentMines(itemData, x, y);

                if (!itemData.TryQueryItem(EntityGridPosition.Of(MineSweeperMapLayers.Items, x, y), out var item) ||
                    !itemResolver.TryUpdateData(item, new MineSweeperMineCount(mines), out var changedItem))
                {
                    throw new InvalidOperationException();
                }

                if (!itemData.TryRemoveItem(item, EntityGridPosition.Of(MineSweeperMapLayers.Items, x, y)) ||
                    !itemData.TryPlaceItem(changedItem, EntityGridPosition.Of(MineSweeperMapLayers.Items, x, y)))
                {
                    throw new InvalidOperationException();
                }
            }
        }

        HashSet<EntityGridPosition> GenerateMinePositions(MineSweeperGameParameter playerData)
        {
            var rng = randomSource.RandomGenerator(playerData.Seed);
            var minePositions = new HashSet<EntityGridPosition>();
            for (int attempt = 0;
                 attempt <= playerData.MineCount * 2 &&
                 minePositions.Count < playerData.MineCount;
                 attempt += 1)
            {
                var x = rng.Next(0, playerData.PlayFieldArea.Width) + 1;
                var y = rng.Next(0, playerData.PlayFieldArea.Height) + 1;
                var pos = EntityGridPosition.Of(MineSweeperMapLayers.Items, x, y);
                minePositions.Add(pos);
            }

            if (minePositions.Count == 0)
            {
                throw new InvalidOperationException();
            }

            return minePositions;
        }

        static void ValidatePlayerData(in MineSweeperGameParameter playerData)
        {
            if (playerData.PlayFieldArea.Width <= 1 ||
                playerData.PlayFieldArea.Height <= 1)
            {
                throw new Exception();
            }

            if (playerData.MineCount <= 0 ||
                playerData.MineCount >= playerData.PlayFieldArea.Width * playerData.PlayFieldArea.Height)
            {
                throw new Exception();
            }
        }

        int CountAdjacentMines(IItemPlacementService<TItemId> itemView, int posX, int posY)
        {
            var count = 0;
            foreach (var dir in AdjacencyRule.EightWay.DirectionsOfNeighbors())
            {
                var coords = dir.ToCoordinates();
                var x = posX + coords.X;
                var y = posY + coords.Y;
                if (itemView.TryQueryItem(EntityGridPosition.Of(MineSweeperMapLayers.Items, x, y), out var item) && itemResolver.TryResolve(item, out var itemType) &&
                    itemType.Id == MineSweeperItemDefinitions.Mine)
                {
                    count += 1;
                }
            }

            return count;
        }
    }
}