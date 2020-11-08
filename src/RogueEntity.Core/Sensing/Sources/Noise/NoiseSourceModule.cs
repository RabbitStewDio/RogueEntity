using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public class NoiseSourceModule : SenseSourceModuleBase<NoiseSense, NoiseSourceDefinition>
    {
        public static readonly string ModuleId = "Core.Senses.Source.Noise";
        
        public static readonly EntityRole NoiseSourceRole = new EntityRole("Role.Core.Senses.Source.Noise");
        public static readonly EntityRole ResistanceDataProviderRole = new EntityRole("Role.Core.Senses.Resistance.Noise");

        public static readonly EntitySystemId PreparationSystemId = "Systems.Core.Senses.Source.Noise.Prepare";
        public static readonly EntitySystemId CollectionGridSystemId = "Systems.Core.Senses.Source.Noise.Collect.Grid";
        public static readonly EntitySystemId CollectionContinuousSystemId = "Systems.Core.Senses.Source.Noise.Collect.Continuous";
        public static readonly EntitySystemId ComputeSystemId = "Systems.Core.Senses.Source.Noise.Compute";
        public static readonly EntitySystemId FinalizeSystemId = "Systems.Core.Senses.Source.Noise.Finalize";

        public static readonly EntitySystemId RegisterResistanceEntitiesId = "Core.Entities.Senses.Resistance.Noise";
        public static readonly EntitySystemId RegisterResistanceSystem = "Core.Systems.Senses.Resistance.Noise.SetUp";
        public static readonly EntitySystemId ExecuteResistanceSystem = "Core.Systems.Senses.Resistance.Noise.Run";
        
        public static readonly EntitySystemId SenseCacheLifecycleId = "Core.Systems.Senses.Cache.Noise.Lifecycle";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Source.Noise";

        public NoiseSourceModule()
        {
            Id = ModuleId;

            DeclareDependencies(ModuleDependency.Of(SensoryCacheModule.ModuleId),
                                ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(NoiseSourceRole).WithImpliedRole(SenseSources.SenseSourceRole).WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);
            ForRole(ResistanceDataProviderRole).WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Noise")]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterEntities);
            ctx.Register(PreparationSystemId, 50000, RegisterPrepareSystem);
            ctx.Register(ComputeSystemId, 58000, RegisterCalculateSystem);
            ctx.Register(FinalizeSystemId, 59000, RegisterCleanUpSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Noise",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeLightCollectionGrid<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                            IModuleInitializer<TGameContext> initializer,
                                                                            EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(CollectionGridSystemId, 55000, RegisterCollectLightsGridSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Noise",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeLightCollectionContinuous<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                  IModuleInitializer<TGameContext> initializer,
                                                                                  EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(CollectionGridSystemId, 55000, RegisterCollectLightsContinuousSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Noise",
                               ConditionalRoles = new[]
                               {
                                   "Role.Core.Position.GridPositioned",
                                   "Role.Core.Senses.Cache.InvalidationSource"
                               })]
        protected void InitializeLightSenseCache<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                        IModuleInitializer<TGameContext> initializer,
                                                                        EntityRole role)
            where TItemId : IEntityKey
        {
            if (serviceResolver.TryResolve(out SenseCacheSetUpSystem<TGameContext> o))
            {
                o.RegisterSense<NoiseSense>();
            }
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Noise",
                               ConditionalRoles = new[]
                               {
                                   "Role.Core.Senses.Resistance.Noise"
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

            ctx.Register(SenseCacheLifecycleId, 500, RegisterSenseResistanceCacheLifeCycle<TGameContext, TItemId, NoiseSense>);
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateSensePhysics(IServiceResolver resolver)
        {
            var physics = resolver.Resolve<INoisePhysicsConfiguration>();
            return (physics.CreateNoisePropagationAlgorithm(), physics.NoisePhysics);
        }
    }
}