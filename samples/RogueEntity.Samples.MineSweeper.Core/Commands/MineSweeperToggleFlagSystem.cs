using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Samples.MineSweeper.Core.Traits;
using System;

namespace RogueEntity.Samples.MineSweeper.Core.Commands
{
    public class MineSweeperToggleFlagSystem<TItemId>
        where TItemId : struct, IEntityKey
    {
        readonly IItemPlacementServiceContext<TItemId> gridMap;
        readonly IItemResolver<TItemId> itemResolver;
        readonly MapBuilder mapBuilder;

        public MineSweeperToggleFlagSystem(IItemPlacementServiceContext<TItemId> gridMap, IItemResolver<TItemId> itemResolver, MapBuilder mapBuilder)
        {
            this.gridMap = gridMap;
            this.itemResolver = itemResolver;
            this.mapBuilder = mapBuilder;
        }

        public void ProcessInputCommand<TActorId>(IEntityViewControl<TActorId> v,
                                                  TActorId k,
                                                  in MineSweeperPlayerData playerData,
                                                  in ToggleFlagCommand revealCommand)
            where TActorId : struct, IEntityKey
        {
            try
            {
                var pos = revealCommand.Position;
                if (!gridMap.TryGetItemPlacementService(MineSweeperMapLayers.Items, out var itemData))
                {
                    throw new InvalidOperationException();
                }

                if (!gridMap.TryGetItemPlacementService(MineSweeperMapLayers.Flags, out var flagData))
                {
                    throw new InvalidOperationException();
                }

                if (!itemData.TryQueryItem(EntityGridPosition.Of(MineSweeperMapLayers.Items, pos.X, pos.Y), out var item) || item.IsEmpty)
                {
                    return;
                }

                flagData.TryQueryItem(EntityGridPosition.Of(MineSweeperMapLayers.Flags, pos.X, pos.Y), out var flag);
                var position = Position.Of(MineSweeperMapLayers.Flags, pos.X, pos.Y);
                if (itemResolver.IsItemType(flag, MineSweeperItemDefinitions.Flag))
                {
                    mapBuilder.Clear(position);
                }
                else
                {
                    mapBuilder.Instantiate(MineSweeperItemDefinitions.Flag, position);
                }
            }
            finally
            {
                v.RemoveComponent<ToggleFlagCommand>(k);
                v.RemoveComponent<CommandInProgress>(k);
            }
        }
    }
}
