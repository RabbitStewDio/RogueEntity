using Microsoft.Xna.Framework;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Runtime;
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
        readonly MapRenderer renderer;
        IBoxPusherMapMetaDataService metaDataService;

        public BoxPusherMapDrawHandler(BoxPusherGame game, BoxPusherInputState sharedState)
        {
            this.sharedState = sharedState;
            this.renderer = new MapRenderer();

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
            renderer.AddGridLayer<ItemReference>(BoxPusherMapLayers.Floor, this.game.ServiceResolver)
                    .WithRenderTemplate(BoxPusherItemDefinitions.EmptyFloor.Tag,
                                        ConsoleRenderData.For(5)
                                                         .WithForeground(Color.White))
                    .WithRenderTemplate(BoxPusherItemDefinitions.TargetZoneFloor.Tag,
                                        ConsoleRenderData.For(4)
                                                         .WithForeground(Color.Green, true));

            renderer.AddGridLayer<ItemReference>(BoxPusherMapLayers.Items, this.game.ServiceResolver)
                    .WithRenderTemplate(BoxPusherItemDefinitions.Wall.Tag,
                                        ConsoleRenderData.For(2)
                                                         .WithForeground(Color.White))
                    .WithRenderTemplate(BoxPusherItemDefinitions.Box.Tag,
                                        ConsoleRenderData.For(3, true)
                                                         .WithForeground(Color.Black)
                                                         .WithBackground(Color.White));

            renderer.AddGridLayer<ActorReference>(BoxPusherMapLayers.Actors, this.game.ServiceResolver)
                    .WithRenderTemplate(BoxPusherItemDefinitions.Player.Tag,
                                        ConsoleRenderData.For(6, true)
                                                         .WithForeground(Color.Yellow));

            metaDataService = this.game.ServiceResolver.Resolve<IBoxPusherMapMetaDataService>();
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

            if (metaDataService.TryGetMapBounds(pos.GridZ, out var m))
            {
                var center = m.Center;
                renderer.Render(pos.WithPosition(center.X, center.Y), console);
            }
            else
            {
                renderer.Render(pos, console);
            }


        }
    }
}
