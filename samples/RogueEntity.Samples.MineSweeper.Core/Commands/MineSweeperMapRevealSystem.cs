using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
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
        where TItemId : IEntityKey
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
            where TActorId : IEntityKey
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
                var flag = flagView[pos.X, pos.Y];
                if (itemResolver.IsItemType(flag, MineSweeperItemDefinitions.Flag))
                {
                    // we dont act if we suspect a mine here.
                    Logger.Debug("Flag set at {Position}", pos);
                    return;
                }

                var item = itemView[pos.X, pos.Y];
                if (itemResolver.IsDestroyed(item) ||
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
                        view[pos.X, pos.Y] = true;
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
            }
        }

        bool IsAreaCleared(IReadOnlyView2D<TItemId> playField,
                           IDynamicDataView2D<bool> discoveryView)
        {
            foreach (var (x,y) in gameParameters.WorldParameter.ValidInputBounds.Contents)
            {
                if (discoveryView[x, y])
                {
                    continue;
                }
                
                if (!itemResolver.IsItemType(playField[x, y], MineSweeperItemDefinitions.Mine))
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
                if (discoveryView[p.X, p.Y])
                {
                    // already discovered
                    continue;
                }

                var playFieldEntity = playField[p.X, p.Y];
                if (playFieldEntity.IsEmpty)
                {
                    continue;
                }

                if (itemResolver.IsItemType(playFieldEntity, MineSweeperItemDefinitions.Mine))
                {
                    continue;
                }

                discoveryView[p.X, p.Y] = true;
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
