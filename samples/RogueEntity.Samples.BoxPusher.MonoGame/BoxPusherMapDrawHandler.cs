using EnTTSharp.Entities;
using Microsoft.Xna.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Services;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Runtime;
using RogueEntity.Core.Utils;
using RogueEntity.SadCons;
using RogueEntity.Samples.BoxPusher.Core;
using SadConsole;
using SadConsole.Components;
using System;
using System.Collections.Generic;
using Console = SadConsole.Console;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherMapDrawHandler : DrawConsoleComponent
    {
        readonly BoxPusherGame game;
        readonly BoxPusherInputState sharedState;

        RenderLayer<ItemReference> floorLayerMapData;
        RenderLayer<ItemReference> itemLayerMapData;
        RenderLayer<ActorReference> actorLayerMapData;

        public BoxPusherMapDrawHandler(BoxPusherGame game, BoxPusherInputState sharedState)
        {
            this.sharedState = sharedState;

            this.game = game;
            if (this.game.Status.IsInitialized())
            {
                OnGameInitialized(this, EventArgs.Empty);
            }
            else
            {
                this.game.GameInitialized += OnGameInitialized;
            }
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            this.floorLayerMapData = RenderLayer<ItemReference>.CreateRendererFor(BoxPusherMapLayers.Floor, this.game.ServiceResolver);
            this.itemLayerMapData = RenderLayer<ItemReference>.CreateRendererFor(BoxPusherMapLayers.Items, this.game.ServiceResolver);
            this.actorLayerMapData = RenderLayer<ActorReference>.CreateRendererFor(BoxPusherMapLayers.Actors, this.game.ServiceResolver);

            this.floorLayerMapData
                .WithRenderTemplate(BoxPusherItemDefinitions.EmptyFloor.Tag, new Cell()
                {
                    Background = Color.Transparent,
                    Foreground = Color.White,
                    Glyph = '.'
                })
                .WithRenderTemplate(BoxPusherItemDefinitions.TargetZoneFloor.Tag, new Cell()
                {
                    Background = Color.Transparent,
                    Foreground = Color.Green,
                    Glyph = ':'
                });
            this.itemLayerMapData
                .WithRenderTemplate(BoxPusherItemDefinitions.Wall.Tag, new Cell()
                {
                    Background = Color.Transparent,
                    Foreground = Color.White,
                    Glyph = '#'
                })
                .WithRenderTemplate(BoxPusherItemDefinitions.Box.Tag, new Cell()
                {
                    Background = Color.White,
                    Foreground = Color.Transparent,
                    Glyph = '+'
                });
            this.actorLayerMapData
                .WithRenderTemplate(BoxPusherItemDefinitions.Player.Tag, new Cell()
                {
                    Background = Color.Transparent,
                    Foreground = Color.Yellow,
                    Glyph = '@'
                });

            this.game.GameInitialized -= OnGameInitialized;
        }

        public override void Draw(Console console, TimeSpan delta)
        {
            if (!sharedState.PlayerObserver.TryGetValue(out var observer))
            {
                console.FillWithRandomGarbage();
                return;
            }

            console.DefaultForeground = Color.Transparent;
            console.DefaultBackground = Color.Transparent;
            console.Clear();

            var pos = observer.Position;
            if (pos.IsInvalid)
            {
                return;
            }

            actorLayerMapData.Render(pos, console);
            itemLayerMapData.Render(pos, console);
            floorLayerMapData.Render(pos, console);
        }


        class RenderLayer<TMapData>
            where TMapData : IEntityKey
        {
            readonly IGridMapDataContext<TMapData> layer;
            readonly IItemResolver<TMapData> itemResolver;
            readonly Dictionary<string, Cell> rendererForTags;

            public RenderLayer(IGridMapDataContext<TMapData> layer,
                               IItemResolver<TMapData> itemResolver)
            {
                this.layer = layer;
                this.itemResolver = itemResolver;
                this.rendererForTags = new Dictionary<string, Cell>();
            }

            public RenderLayer<TMapData> WithRenderTemplate(string tag, Cell cell)
            {
                this.rendererForTags[tag] = cell;
                return this;
            }

            public bool Render(Position observerAtCenter, Console console)
            {
                if (observerAtCenter.IsInvalid)
                {
                    return false;
                }

                if (!layer.TryGetView(observerAtCenter.GridZ, out var view))
                {
                    return false;
                }


                var mapOrigin = observerAtCenter.ToGridXY() - new Position2D(console.Width / 2, console.Height / 2);
                foreach (var (x, y) in new RectangleContents(0, 0, console.Width, console.Height))
                {
                    var item = view[mapOrigin.X + x, mapOrigin.Y + y];
                    if (item.IsEmpty)
                    {
                        continue;
                    }

                    if (!itemResolver.TryResolve(item, out var itemDeclaration) ||
                        !rendererForTags.TryGetValue(itemDeclaration.Tag, out var cellTemplate))
                    {
                        continue;
                    }

                    console.GetCellAt(x, y).MergeAppearanceFrom(cellTemplate);
                }

                return true;
            }

            public static RenderLayer<TMapData> CreateRendererFor(MapLayer layer, IServiceResolver serviceResolver)
            {
                var map = serviceResolver.Resolve<IGridMapContext<TMapData>>();
                if (map.TryGetGridDataFor(layer, out var layerData))
                {
                    return new RenderLayer<TMapData>(layerData, serviceResolver.Resolve<IItemResolver<TMapData>>());
                }

                throw new ArgumentException("Unable to create renderer for layer " + layer);
            }
        }
    }
}
