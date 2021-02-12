using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Generator;
using System;

namespace RogueEntity.Simple.MineSweeper
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
            v.RemoveComponent<ToggleFlagCommand>(k);
            
            if (!playerData.ActiveArea.Contains(revealCommand.Position))
            {
                return;
            }
            
            if (!gridMap.TryGetGridDataFor(MineSweeperMapLayers.Flags, out var flagData) ||
                !flagData.TryGetWritableView(0, out var flagView))
            {
                throw new InvalidOperationException();
            }

            var flag = flagView[revealCommand.Position.X, revealCommand.Position.Y];
            var position = Position.Of(MineSweeperMapLayers.Flags, revealCommand.Position.X, revealCommand.Position.Y);
            if (itemResolver.IsItemType(flag, MineSweeperItemDefinitions.Flag))
            {
                mapBuilder.Clear(position);
            }
            else
            {
                mapBuilder.Instantiate(MineSweeperItemDefinitions.Flag, position);
            }
        }
    }
}
