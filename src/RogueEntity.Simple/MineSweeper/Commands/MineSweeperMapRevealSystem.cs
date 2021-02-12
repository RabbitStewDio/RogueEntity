using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Discovery;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;

namespace RogueEntity.Simple.MineSweeper
{
    public class MineSweeperMapRevealSystem<TItemId>
        where TItemId : IEntityKey
    {
        readonly IGridMapContext<TItemId> gridMap;
        readonly IItemResolver<TItemId> itemResolver;

        public MineSweeperMapRevealSystem(IGridMapContext<TItemId> gridMap, IItemResolver<TItemId> itemResolver)
        {
            this.gridMap = gridMap;
            this.itemResolver = itemResolver;
        }

        public void ProcessInputCommand<TActorId>(IEntityViewControl<TActorId> v, TActorId k, 
                                                  in MineSweeperPlayerData playerData,
                                                  in DiscoveryMapData discoveryMap,
                                                  in RevealMapPositionCommand revealCommand)
            where TActorId : IEntityKey
        {
            v.RemoveComponent<RevealMapPositionCommand>(k);
            if (!playerData.ActiveArea.Contains(revealCommand.Position))
            {
                return;
            }

            if (!gridMap.TryGetGridDataFor(MineSweeperMapLayers.Flags, out var flagData) ||
                !flagData.TryGetWritableView(0, out var flagView))
            {
                throw new InvalidOperationException();
            }

            if (!gridMap.TryGetGridDataFor(MineSweeperMapLayers.Items, out var itemData) ||
                !itemData.TryGetWritableView(0, out var itemView, DataViewCreateMode.CreateMissing))
            {
                throw new InvalidOperationException();
            }
            
            var flag = flagView[revealCommand.Position.X, revealCommand.Position.Y];
            if (itemResolver.IsItemType(flag, MineSweeperItemDefinitions.Flag))
            {
                // we dont act if we suspect a mine here.
                return;
            }

            var item = itemView[revealCommand.Position.X, revealCommand.Position.Y];
            if (itemResolver.IsDestroyed(item) || 
                itemResolver.IsItemType(item, MineSweeperItemDefinitions.Wall))
            {
                // do nothing
                return;
            }

            if (itemResolver.IsItemType(item, MineSweeperItemDefinitions.Mine))
            {
                // boom!
                playerData.ExplodedPosition = EntityGridPosition.Of(MineSweeperMapLayers.Items, revealCommand.Position.X, revealCommand.Position.Y);
                return;
            }

            ExpandVisibleArea(revealCommand.Position, itemView);

        }

        void ExpandVisibleArea(Position2D revealCommandPosition, 
                               IDynamicDataView2D<TItemId> dynamicDataView2D)
        {
            
        }
    }
}
