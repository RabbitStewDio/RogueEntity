using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Generator;
using System;

namespace RogueEntity.Samples.MineSweeper.Core.Commands
{
    public class MineSweeperToggleFlagSystem<TItemId>
        where TItemId : IEntityKey
    {
        readonly IGridMapContext<TItemId> gridMap;
        readonly IItemResolver<TItemId> itemResolver;
        readonly MapBuilder mapBuilder;

        public MineSweeperToggleFlagSystem(IGridMapContext<TItemId> gridMap, IItemResolver<TItemId> itemResolver, MapBuilder mapBuilder)
        {
            this.gridMap = gridMap;
            this.itemResolver = itemResolver;
            this.mapBuilder = mapBuilder;
        }

        public void ProcessInputCommand<TActorId>(IEntityViewControl<TActorId> v,
                                                  TActorId k,
                                                  in MineSweeperPlayerData playerData,
                                                  in ToggleFlagCommand revealCommand)
            where TActorId : IEntityKey
        {
            try
            {
                var pos = revealCommand.Position;
                if (!gridMap.TryGetGridDataFor(MineSweeperMapLayers.Items, out var itemData) ||
                    !itemData.TryGetView(0, out var itemView))
                {
                    throw new InvalidOperationException();
                }

                if (!gridMap.TryGetGridDataFor(MineSweeperMapLayers.Flags, out var flagData) ||
                    !flagData.TryGetWritableView(0, out var flagView))
                {
                    throw new InvalidOperationException();
                }

                if (itemView[pos.X, pos.Y].IsEmpty)
                {
                    return;
                }

                var flag = flagView[pos.X, pos.Y];
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
            }
        }
    }
}
