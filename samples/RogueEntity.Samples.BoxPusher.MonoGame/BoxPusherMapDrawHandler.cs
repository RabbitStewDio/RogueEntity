using Microsoft.Xna.Framework;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Runtime;
using RogueEntity.Generator;
using RogueEntity.SadCons.MapRendering;
using RogueEntity.Samples.BoxPusher.Core;
using SadConsole.Components;
using System;
using Console = SadConsole.Console;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherMapDrawHandler : DrawConsoleComponent
    {
        readonly BoxPusherGame game;
        readonly BoxPusherInputState sharedState;
        IMapRegionMetaDataService<int> metaDataService;
        readonly BasicMapRenderer br;

        public BoxPusherMapDrawHandler(BoxPusherGame game, BoxPusherInputState sharedState)
        {
            this.sharedState = sharedState;
            // this.renderer = new MapRenderer();
            this.br = new BasicMapRenderer();

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
            br.DefineRenderLayer(RenderLayers.FromGrid(this.game.ServiceResolver.Resolve<IGridMapContext<ItemReference>>())
                                             .ForMapLayer(BoxPusherMapLayers.Floor)
                                             .WithConverter(RenderLayers.StandardTagConverter<ItemReference>(this.game.ServiceResolver))
                                             .Build());
            br.DefineRenderLayer(RenderLayers.FromGrid(this.game.ServiceResolver.Resolve<IGridMapContext<ItemReference>>())
                                             .ForMapLayer(BoxPusherMapLayers.Items)
                                             .WithConverter(RenderLayers.StandardTagConverter<ItemReference>(this.game.ServiceResolver))
                                             .Build());
            br.DefineRenderLayer(RenderLayers.FromGrid(this.game.ServiceResolver.Resolve<IGridMapContext<ActorReference>>())
                                             .ForMapLayer(BoxPusherMapLayers.Actors)
                                             .WithConverter(RenderLayers.StandardTagConverter<ActorReference>(this.game.ServiceResolver))
                                             .Build());

            br.Add(RenderMatchers.Match(BoxPusherMapLayers.Floor, BoxPusherItemDefinitions.EmptyFloor.Tag)
                                 .As(ConsoleRenderData.For(5)
                                                      .WithForeground(Color.White)));
            br.Add(RenderMatchers.Match(BoxPusherMapLayers.Floor, BoxPusherItemDefinitions.TargetZoneFloor.Tag)
                                 .As(ConsoleRenderData.For(4)
                                                      .WithForeground(Color.Green)));
            br.Add(RenderMatchers.Match(BoxPusherMapLayers.Floor, BoxPusherItemDefinitions.TargetZoneFloor.Tag)
                                 .And(BoxPusherMapLayers.Items, BoxPusherItemDefinitions.Box.Tag)
                                 .As(ConsoleRenderData.For(3)
                                                      .WithForeground(Color.Black)
                                                      .WithBackground(Color.Green)));
            br.Add(RenderMatchers.Match(BoxPusherMapLayers.Items, BoxPusherItemDefinitions.Box.Tag)
                                 .As(ConsoleRenderData.For(3)
                                                      .WithForeground(Color.Black)
                                                      .WithBackground(Color.White)));
            br.Add(RenderMatchers.Match(BoxPusherMapLayers.Items, BoxPusherItemDefinitions.Wall.Tag)
                                 .As(ConsoleRenderData.For(2)
                                                      .WithForeground(Color.White)
                                                      .WithBackground(Color.Black)));
            br.Add(RenderMatchers.Match(BoxPusherMapLayers.Actors, BoxPusherItemDefinitions.Player.Tag)
                                 .As(ConsoleRenderData.For(6)
                                                      .WithForeground(Color.Yellow)
                                                      .WithBackground(Color.Black)));

            metaDataService = this.game.ServiceResolver.Resolve<IMapRegionMetaDataService<int>>();
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
                console.FillWithRandomGarbage();
                return;
            }

            if (metaDataService.TryGetRegionBounds(pos.GridZ, out var m))
            {
                var center = m.ToLayerSlice().Center;
                br.Render(pos.WithPosition(center.X, center.Y), console);
            }
            else
            {
                br.Render(pos, console);
            }
        }
    }
}
