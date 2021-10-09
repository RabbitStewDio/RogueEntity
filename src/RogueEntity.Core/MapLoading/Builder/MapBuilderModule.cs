using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.MapLayers;
using Serilog;
using System.Linq;

namespace RogueEntity.Core.MapLoading.Builder
{
    public class MapBuilderModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.MapBuilderModule";
        public static readonly EntitySystemId RegisterMapBuilderSystemId = "Systems.Core.MapBuilder.RegisterMapLayers";
        public static readonly EntitySystemId PrintMapBuilderConfigurationId = "Systems.Core.MapBuilder.PrintConfiguration";

        readonly ILogger Logger = SLog.ForContext<MapBuilderModule>();
        
        public MapBuilderModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core.MapBuilder";
            Name = "RogueEntity Map Builder Module";
            Description = "Provides a map builder to generalize item placement and randomization";
            IsFrameworkModule = true;
        }

        [EntityRoleInitializer("Role.Core.Position.GridPositioned")]
        protected void InitializeGridPositioned<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                          IModuleInitializer initializer,
                                                          EntityRole role)
            where TActorId : IEntityKey
        {
            initializer.RegisterFinalizer(PrintMapBuilderConfigurationId, 999_000, PrintMapBuilderStructure);
            
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(RegisterMapBuilderSystemId, 0, RegisterMapBuilder);
        }

        void RegisterMapBuilder<TActorId>(in ModuleEntityInitializationParameter<TActorId> ip, EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var mb = GetOrCreateMapBuilder(ip.ServiceResolver);
            
            if (!ip.ServiceResolver.TryResolve<IItemResolver<TActorId>>(out var itemResolver))
            {
                return;
            }

            var mapLayers = itemResolver.ItemRegistry.QueryDesignTimeTrait<MapLayerPreference>()
                                        .SelectMany(e => e.Item2.AcceptableLayers)
                                        .Distinct()
                                        .ToList();
            if (mapLayers.Count == 0)
            {
                return;
            }

            foreach (var ml in mapLayers)
            {
                Logger.Debug("Automatically registering map-builder layer {Layer} for entity type {EntityType}", ml, typeof(TActorId));
                mb.WithLayer<TActorId>(ml, ip.ServiceResolver);
            }
        }

        MapBuilder GetOrCreateMapBuilder(IServiceResolver r)
        {
            if (r.TryResolve(out MapBuilder b))
            {
                return b;
            }

            b = new MapBuilder();
            r.Store(b);
            return b;
        }

        protected void PrintMapBuilderStructure(in ModuleInitializationParameter initParam, IGameLoopSystemRegistration context)
        {
            var r = initParam.ServiceResolver;
            if (!r.TryResolve(out MapBuilder b))
            {
                return;
            }

            Logger.Debug("MapBuilder final layer structure: {Layers}", b.Layers);
        }
    }
}
