using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Inventory;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Discovery;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources;

namespace RogueEntity.Simple.BoxPusher
{
    public class BoxPusherModule : ModuleBase
    {
        public static EntitySystemId SetUpGameSenseResistancesId = "System.Game.BoxPusher.SenseResistanceSystems";
        
        public static EntityRole MovableItemRole = new EntityRole("Role.Game.BoxPusher.MovableItem");
        public static EntityRole FloorRole = new EntityRole("Role.Game.BoxPusher.Floor");
        public static EntityRole ActorRole = new EntityRole("Role.Game.BoxPusher.Actors");

        public BoxPusherModule()
        {
            Id = "Game.BoxPusher";

            DeclareDependencies(ModuleDependency.Of(InventoryModule.ModuleId),
                                ModuleDependency.Of(SensoryResistanceModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId),
                                ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(CoreModule.ModuleId));
            
            DeclareEntity<ItemReference>(MovableItemRole)
                .WithImpliedRole(CoreModule.ItemRole)
                .WithImpliedRole(InventoryModule.ContainedItemRole)
                .WithImpliedRole(SensoryResistanceModule.ResistanceDataProviderRole);

            DeclareEntity<ItemReference>(FloorRole)
                .WithImpliedRole(CoreModule.ItemRole)
                .WithImpliedRole(PositionModule.GridPositionedRole);

            DeclareEntity<ActorReference>(ActorRole)
                .WithImpliedRole(CoreModule.ItemRole)
                .WithImpliedRole(CoreModule.PlayerRole)
                .WithImpliedRole(InventoryModule.ContainerRole)
                .WithImpliedRole(SenseDiscoveryModule.DiscoveryActorRole);

            DeclareRelation<ActorReference, ItemReference>(InventoryModule.ContainsRelation);
            
            RequireRole(MovableItemRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Resistance.ResistanceDataProvider")]
        protected void InitializeRole<TGameContext, TItemId>(IModuleInitializer<TGameContext> initializer, EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TGameContext, TItemId>
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SetUpGameSenseResistancesId, 1100, RegisterResistanceSystemConfiguration);
        }

        void RegisterResistanceSystemConfiguration<TGameContext, TItemId>(IServiceResolver serviceResolver, 
                                                                          IGameLoopSystemRegistration<TGameContext> context, 
                                                                          EntityRegistry<TItemId> registry, 
                                                                          ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
            where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TGameContext, TItemId>
        {
            if (serviceResolver.TryResolve(out SenseStateCacheProvider cache))
            {
                
            }
            
            var factory = serviceResolver.Resolve<ISensePropertiesSystem<TGameContext>>();
            factory.AddLayer<TGameContext, TItemId>(BoxPusherMapLayers.Items);
        }
    }
}