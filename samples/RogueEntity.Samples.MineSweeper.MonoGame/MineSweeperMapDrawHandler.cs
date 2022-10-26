using Microsoft.Xna.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Runtime;
using RogueEntity.Core.Sensing.Discovery;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.SadCons;
using RogueEntity.Samples.MineSweeper.Core;
using RogueEntity.Samples.MineSweeper.Core.Traits;
using SadConsole;
using SadConsole.Components;
using System;
using Console = SadConsole.Console;

namespace RogueEntity.Samples.MineSweeper.MonoGame
{
    public class MineSweeperMapDrawHandler: DrawConsoleComponent
    {
        readonly MineSweeperInputState inputState;
        readonly MineSweeperGame game;
        IGridMapContext<ItemReference> itemMap;
        IItemResolver<ActorReference> actorResolver;
        IItemResolver<ItemReference> itemResolver;

        public MineSweeperMapDrawHandler(MineSweeperInputState inputState, 
                                         MineSweeperGame game)
        {
            this.inputState = inputState;
            this.game = game;
            if (!game.Status.IsInitialized())
            {
                this.game.GameInitialized += OnGameInitialized;
            }
            else
            {
                OnGameInitialized(game, EventArgs.Empty);
            }
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            itemMap = game.ServiceResolver.Resolve<IGridMapContext<ItemReference>>();
            actorResolver = game.ServiceResolver.Resolve<IItemResolver<ActorReference>>();
            itemResolver = game.ServiceResolver.Resolve<IItemResolver<ItemReference>>();
            
            this.game.GameInitialized -= OnGameInitialized;
        }

        public override void Draw(Console console, TimeSpan delta)
        {
            if (!console.IsVisible)
            {
                System.Console.WriteLine("Not visible");
                return;
            }

            if (!(console is ScrollingConsole scrollingConsole))
            {
                System.Console.WriteLine("Not a scrolling console");
                return;
            }

            if (!itemMap.TryGetGridDataFor(MineSweeperMapLayers.Items, out var itemLayer) ||
                !itemLayer.TryGetView(0, out var itemView))
            {
                System.Console.WriteLine("Have no item view");
                return;
            }

            if (!itemMap.TryGetGridDataFor(MineSweeperMapLayers.Flags, out var flagLayer) ||
                !flagLayer.TryGetView(0, out var flagView))
            {
                System.Console.WriteLine("Have no flag view");
                return;
            }
            
            if (!game.PlayerData.TryGetValue(out var playerData))
            {
                System.Console.WriteLine("Have no player data");
                return;
            }

            console.Clear();
            
            var playerEntity = playerData.EntityId;
            var discoveryView = QueryDiscoveryMap(playerEntity);
            
            var viewPort = scrollingConsole.ViewPort;
            foreach (var (x,y) in new RectangleContents(viewPort.X, viewPort.Y, viewPort.Width, viewPort.Height))
            {
                if (discoveryView.TryGet(x, y, out var discovered) && discovered)
                {
                    // render item
                    if (!itemView.TryGet(x, y, out var item) || item.IsEmpty || itemResolver.IsItemType(item, MineSweeperItemDefinitions.Wall))
                    {
                        console.SetGlyph(x, y, '#', Color.White, Color.Black);
                    }
                    else if (itemResolver.IsItemType(item, MineSweeperItemDefinitions.Mine))
                    {
                        console.SetGlyph(x, y, '*', Color.Red, Color.Black);
                    }
                    else if (itemResolver.TryQueryData(item, out MineSweeperMineCount mc))
                    {
                        if (mc.Count > 0)
                        {
                            console.SetGlyph(x, y, '0' + mc.Count, Color.Cyan, Color.Black);
                        }
                        else
                        {
                            console.SetGlyph(x, y, '.', Color.White, Color.Black);
                        }
                    }
                    else
                    {
                        console.SetGlyph(x, y, '#', Color.Gray, Color.Black);
                    }
                        
                }
                else
                {
                    // render flagged
                    if (!flagView.TryGet(x, y, out var flagState) || flagState.IsEmpty)
                    {
                        console.SetGlyph(x, y, '?', new Color(10, 10, 10), Color.Black);
                    }
                    else
                    {
                        console.SetGlyph(x, y, 'F', Color.White, Color.Black);
                    }
                }
            }

            if (inputState.MouseHoverPosition.TryGetValue(out var pos))
            {
                console.DrawCursor(pos.X, pos.Y);
            }
        }

        IReadOnlyView2D<bool> QueryDiscoveryMap(ActorReference playerEntity)
        {
            if (actorResolver.TryQueryData(playerEntity, out IDiscoveryMap discoveryMap) &&
                discoveryMap.TryGetView(0, out var discoveryView))
            {
                return discoveryView;
            }

            return new ConstantDataView2D<bool>(false);
        }
    }
}
