using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Discovery;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Samples.MineSweeper.Core.Services;
using RogueEntity.Samples.MineSweeper.Core.Traits;
using Serilog;
using System;
using System.Collections.Generic;

namespace RogueEntity.Samples.MineSweeper.Core.Commands
{
    public class MineSweeperMapRevealSystem<TItemId>
        where TItemId : struct, IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<MineSweeperMapRevealSystem<TItemId>>();
        
        readonly IGridMapContext<TItemId> gridMap;
        readonly IItemResolver<TItemId> itemResolver;
        readonly IMineSweeperGameParameterService gameParameters;
        readonly Stack<Position2D> processingQueue;

        public MineSweeperMapRevealSystem(IGridMapContext<TItemId> gridMap, 
                                          IItemResolver<TItemId> itemResolver,
                                          IMineSweeperGameParameterService gameParameters)
        {
            this.gridMap = gridMap;
            this.itemResolver = itemResolver;
            this.gameParameters = gameParameters;
            this.processingQueue = new Stack<Position2D>();
        }

        public void ProcessInputCommand<TActorId>(IEntityViewControl<TActorId> v, TActorId k, 
                                                  in DiscoveryMapData discoveryMap,
                                                  in RevealMapPositionCommand revealCommand,
                                                  ref MineSweeperPlayerData playerData)
            where TActorId : struct, IEntityKey
        {
            try
            {
                if (!gridMap.TryGetGridDataFor(MineSweeperMapLayers.Flags, out var flagData) ||
                    !flagData.TryGetView(0, out var flagView))
                {
                    throw new InvalidOperationException();
                }

                if (!gridMap.TryGetGridDataFor(MineSweeperMapLayers.Items, out var itemData) ||
                    !itemData.TryGetView(0, out var itemView))
                {
                    throw new InvalidOperationException();
                }

                var pos = revealCommand.Position;
                if (flagView.TryGet(pos.X, pos.Y, out var flag) && 
                    itemResolver.IsItemType(flag, MineSweeperItemDefinitions.Flag))
                {
                    // we dont act if we suspect a mine here.
                    Logger.Debug("Flag set at {Position}", pos);
                    return;
                }

                if (!itemView.TryGet(pos.X, pos.Y, out var item) ||
                    itemResolver.IsDestroyed(item) ||
                    itemResolver.IsItemType(item, MineSweeperItemDefinitions.Wall))
                {
                    // do nothing
                    Logger.Debug("Wall at {Position}", pos);
                    return;
                }

                if (itemResolver.IsItemType(item, MineSweeperItemDefinitions.Mine))
                {
                    // boom!
                    playerData = playerData.WithExplodedPosition(EntityGridPosition.Of(MineSweeperMapLayers.Items, pos.X, pos.Y));
                    if (discoveryMap.TryGetWritableView(0, out var view))
                    {
                        view.TrySet(pos.X, pos.Y, true);
                    }
                    Logger.Debug("Mine at {Position}", pos);
                    return;
                }

                if (discoveryMap.TryGetWritableView(0, out var discoveryMapView, DataViewCreateMode.CreateMissing))
                {
                    Logger.Debug("Expanding view from position {Position}", pos);
                    ExpandVisibleArea(pos, itemView, discoveryMapView);
                }
                else
                {
                    Logger.Debug("Unable to handle input for position {Position}", pos);
                }

                if (IsAreaCleared(itemView, discoveryMapView))
                {
                    playerData = playerData.WithAreaCleared();
                }
            }
            finally
            {
                v.RemoveComponent<RevealMapPositionCommand>(k);
                v.RemoveComponent<CommandInProgress>(k);
            }
        }

        bool IsAreaCleared(IReadOnlyView2D<TItemId> playField,
                           IDynamicDataView2D<bool> discoveryView)
        {
            foreach (var (x,y) in gameParameters.WorldParameter.ValidInputBounds.Contents)
            {
                if (discoveryView.TryGet(x, y, out var discovered) && discovered)
                {
                    continue;
                }
                
                if (playField.TryGet(x, y, out var maybeMine) && !itemResolver.IsItemType(maybeMine, MineSweeperItemDefinitions.Mine))
                {
                    return false;
                }
            }
            
            return true;
        }


        void ExpandVisibleArea(Position2D pos,
                               IReadOnlyView2D<TItemId> playField,
                               IDynamicDataView2D<bool> discoveryView)
        {
            
            processingQueue.Clear();
            processingQueue.Push(pos);
            while (processingQueue.Count > 0)
            {
                var p = processingQueue.Pop();
                if (!discoveryView.TryGet(p.X, p.Y, out var discovered) || discovered)
                {
                    // already discovered
                    continue;
                }

                if (!playField.TryGet(p.X, p.Y, out var playFieldEntity) || playFieldEntity.IsEmpty)
                {
                    continue;
                }

                if (itemResolver.IsItemType(playFieldEntity, MineSweeperItemDefinitions.Mine))
                {
                    continue;
                }

                discoveryView.TrySet(p.X, p.Y, true);
                if (itemResolver.IsItemType(playFieldEntity, MineSweeperItemDefinitions.Wall))
                {
                    continue;
                }

                if (itemResolver.TryQueryData(playFieldEntity, out MineSweeperMineCount mc) &&
                    mc.Count > 0)
                {
                    continue;
                }

                foreach (var d in AdjacencyRule.EightWay.DirectionsOfNeighbors())
                {
                    var c = p + d;
                    processingQueue.Push(c);
                }
            }
        }
    }
}
