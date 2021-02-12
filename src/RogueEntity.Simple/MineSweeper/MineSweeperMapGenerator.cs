using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Generator;
using System;
using System.Collections.Generic;

namespace RogueEntity.Simple.MineSweeper
{
    public class MineSweeperMapGenerator<TItemId>
        where TItemId : IEntityKey
    {
        readonly IEntityRandomGeneratorSource randomSource;
        readonly IGridMapContext<TItemId> gridMap;
        readonly IItemResolver<TItemId> itemResolver;
        readonly MapBuilder mapBuilder;

        public MineSweeperMapGenerator(IEntityRandomGeneratorSource randomSource, 
                                  IGridMapContext<TItemId> gridMap, 
                                  IItemResolver<TItemId> itemResolver, 
                                  MapBuilder mapBuilder)
        {
            this.randomSource = randomSource;
            this.gridMap = gridMap;
            this.itemResolver = itemResolver;
            this.mapBuilder = mapBuilder;
        }

        public void Activate<TActorId>(IEntityViewControl<TActorId> v, TActorId k, in MineSweeperPlayerData playerData)
            where TActorId : IEntityKey
        {
            if (!gridMap.TryGetGridDataFor(MineSweeperMapLayers.Flags, out var flagData))
            {
                throw new InvalidOperationException();
            }

            if (!gridMap.TryGetGridDataFor(MineSweeperMapLayers.Items, out var itemData) ||
                !itemData.TryGetWritableView(0, out var itemView, DataViewCreateMode.CreateMissing))
            {
                throw new InvalidOperationException();
            }

            flagData.Clear();
            itemData.Clear();

            ValidatePlayerData(playerData);

            // Define the playing field
            // which is surrounded by a wall
            mapBuilder.Draw(MineSweeperMapLayers.Items, 0,
                            new Rectangle(0, 0, playerData.PlayField.Width + 2, playerData.PlayField.Height + 2),
                            MineSweeperItemDefinitions.Wall);
            
            // with a random number of mines
            var minePositions = GenerateMinePositions(playerData);
            foreach (var pos in minePositions)
            {
                mapBuilder.Instantiate(MineSweeperItemDefinitions.Mine, Position.From(pos));
            }
            
            // and finally precompute the neighbourhood cells indicating how many mines are near by
            foreach (var (x, y) in new Rectangle(1, 1, playerData.PlayField.Width, playerData.PlayField.Height).Contents)
            {
                if (!mapBuilder.Instantiate(MineSweeperItemDefinitions.Floor, Position.Of(MineSweeperMapLayers.Items, x, y)))
                {
                    continue;
                }

                var item = itemView[x, y];
                var mines = CountMines(itemView, x, y);
                if (!itemResolver.TryUpdateData(item, new MineSweeperMineCount(mines), out var changedItem))
                {
                    throw new InvalidOperationException();
                }

                itemView[x, y] = changedItem;
            }
        }

        HashSet<EntityGridPosition> GenerateMinePositions(MineSweeperPlayerData playerData) 
        {
            var rng = randomSource.RandomGenerator(playerData.Seed);
            var minePositions = new HashSet<EntityGridPosition>();
            for (int attempt = 0;
                 attempt <= playerData.MineCount * 2 &&
                 minePositions.Count < playerData.MineCount;
                 attempt += 1)
            {
                var x = rng.Next(0, playerData.PlayField.Width) + 1;
                var y = rng.Next(0, playerData.PlayField.Height) + 1;
                var pos = EntityGridPosition.Of(MineSweeperMapLayers.Items, x, y);
                minePositions.Add(pos);
            }

            if (minePositions.Count == 0)
            {
                throw new InvalidOperationException();
            }

            return minePositions;
        }

        static void ValidatePlayerData(MineSweeperPlayerData playerData)
        {
            if (playerData.PlayField.Width <= 1 ||
                playerData.PlayField.Height <= 1)
            {
                playerData.PlayField = new Dimension(10, 10);
            }

            if (playerData.MineCount <= 0 ||
                playerData.MineCount >= playerData.PlayField.Width * playerData.PlayField.Height)
            {
                playerData.MineCount = Math.Max(1, (int) (playerData.PlayField.Width * playerData.PlayField.Height * 0.3f));
            }
        }

        int CountMines(IView2D<TItemId> itemView, int posX, int posY)
        {
            var count = 0;
            foreach (var dir in AdjacencyRule.EightWay.DirectionsOfNeighbors())
            {
                var coords = dir.ToCoordinates();
                var x = posX + coords.X;
                var y = posY + coords.Y;
                var item = itemView[x, y];
                if (itemResolver.TryResolve(item, out var itemType) &&
                    itemType.Id == MineSweeperItemDefinitions.Mine)
                {
                    count += 1;
                }
            }
            
            return count;
        }
    }
}
