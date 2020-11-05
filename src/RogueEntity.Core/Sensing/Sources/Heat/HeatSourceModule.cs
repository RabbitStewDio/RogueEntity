using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Directions;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    public class HeatSourceModule : SenseSourceModuleBase<TemperatureSense, HeatSourceDefinition>
    {
        public static readonly string ModuleId = "Core.Senses.Source.Temperature";
        public static readonly EntityRole HeatSourceRole = new EntityRole("Role.Core.Senses.Source.Temperature");
        public static readonly EntityRole ResistanceDataProviderRole = new EntityRole("Role.Core.Senses.Resistance.Temperature");

        public static readonly EntitySystemId PreparationSystemId = "Systems.Core.Senses.Source.Temperature.Prepare";
        public static readonly EntitySystemId CollectionGridSystemId = "Systems.Core.Senses.Source.Temperature.Collect.Grid";
        public static readonly EntitySystemId CollectionContinuousSystemId = "Systems.Core.Senses.Source.Temperature.Collect.Continuous";
        public static readonly EntitySystemId ComputeSystemId = "Systems.Core.Senses.Source.Temperature.Compute";
        public static readonly EntitySystemId FinalizeSystemId = "Systems.Core.Senses.Source.Temperature.Finalize";

        public static readonly EntitySystemId RegisterResistanceEntitiesId = "Core.Entities.Senses.Resistance.Temperature";
        public static readonly EntitySystemId RegisterResistanceSystem = "Core.Systems.Senses.Resistance.Temperature.SetUp";
        public static readonly EntitySystemId ExecuteResistanceSystem = "Core.Systems.Senses.Resistance.Temperature.Run";

        public static readonly EntitySystemId SenseCacheLifecycleId = "Core.Systems.Senses.Cache.Temperature.Lifecycle";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Source.Temperature";

        public HeatSourceModule()
        {
            Id = ModuleId;

            DeclareDependencies(ModuleDependency.Of(SensoryCacheModule.ModuleId),
                                ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(HeatSourceRole).WithImpliedRole(SenseSources.SenseSourceRole).WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);
            ForRole(ResistanceDataProviderRole).WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Temperature")]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : ITimeContext
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterEntities);
            ctx.Register(PreparationSystemId, 50000, RegisterPrepareSystem);
            ctx.Register(ComputeSystemId, 58000, RegisterCalculateSystem);
            ctx.Register(FinalizeSystemId, 59000, RegisterCleanUpSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Temperature",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeLightCollectionGrid<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                            IModuleInitializer<TGameContext> initializer,
                                                                            EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(CollectionGridSystemId, 55000, RegisterCollectLightsGridSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Temperature",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeLightCollectionContinuous<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                  IModuleInitializer<TGameContext> initializer,
                                                                                  EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(CollectionGridSystemId, 55000, RegisterCollectLightsContinuousSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Temperature",
                               ConditionalRoles = new[]
                               {
                                   "Role.Core.Position.GridPositioned",
                                   "Role.Core.Senses.Cache.InvalidationSource"
                               })]
        protected void InitializeLightSenseCache<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                        IModuleInitializer<TGameContext> initializer,
                                                                        EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : IGridMapContext<TItemId>
        {
            if (serviceResolver.TryResolve(out SenseCacheSetUpSystem<TGameContext> o))
            {
                o.RegisterSense<TemperatureSense>();
            }
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Temperature",
                               ConditionalRoles = new[]
                               {
                                   "Role.Core.Senses.Resistance.Temperature"
                               })]
        protected void InitializeResistanceRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                       IModuleInitializer<TGameContext> initializer,
                                                                       EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterResistanceEntitiesId, 0, RegisterEntities);
            ctx.Register(ExecuteResistanceSystem, 51000, RegisterResistanceSystemExecution);
            ctx.Register(ExecuteResistanceSystem, 52000, RegisterProcessSenseDirectionalitySystem);
            ctx.Register(RegisterResistanceSystem, 500, RegisterResistanceSystemLifecycle);
            
            ctx.Register(SenseCacheLifecycleId, 500, RegisterSenseResistanceCacheLifeCycle<TGameContext, TItemId, TemperatureSense>);
        }

        protected override SenseSourceSystem<TemperatureSense, HeatSourceDefinition> GetOrCreateSenseSystem<TGameContext, TItemId>(IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out SenseSourceSystem<TemperatureSense, HeatSourceDefinition> ls))
            {
                var physics = serviceResolver.Resolve<IHeatPhysicsConfiguration>();
                ls = new HeatSourceSystem(serviceResolver.ResolveToReference<IReadOnlyDynamicDataView3D<SensoryResistance<TemperatureSense>>>(),
                                    serviceResolver.ResolveToReference<IGlobalSenseStateCacheProvider>(),
                                    serviceResolver.ResolveToReference<ITimeSource>(),
                                    serviceResolver.Resolve<ISensoryResistanceDirectionView<TemperatureSense>>(),
                                    serviceResolver.Resolve<ISenseStateCacheControl>(),
                                    physics.CreateHeatPropagationAlgorithm(),
                                    physics);
                serviceResolver.Store(ls);
            }

            return ls;
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateSensePhysics(IServiceResolver resolver)
        {
            var physics = resolver.Resolve<IHeatPhysicsConfiguration>();
            return (physics.CreateHeatPropagationAlgorithm(), physics.HeatPhysics);
        }

    }
}