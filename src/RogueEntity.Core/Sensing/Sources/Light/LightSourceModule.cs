using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    /// <summary>
    ///   Registers light source calculation entities.
    /// </summary>
    /// <remarks>
    ///   Defines the following systems:
    ///
    ///   5000 - preparation: Clear collected sources, fetch current time, etc.
    ///   5700 - collect all sense sources that are dirty. Specialized handling for grid and continuous positions.
    ///   5800 - recompute those collected sense sources.
    ///   5900 - clean up, mark all processed sources as clean.
    /// </remarks>
    public class LightSourceModule : SenseSourceModuleBase<VisionSense, LightSourceDefinition>
    {
        public static readonly string ModuleId = "Core.Senses.Source.Light";
        public static readonly EntityRole LightSourceRole = new EntityRole("Role.Core.Senses.Source.Light");
        public static readonly EntityRole ResistanceDataProviderRole = new EntityRole("Role.Core.Senses.Resistance.Light");

        public static readonly EntitySystemId LightsPreparationSystemId = "Systems.Core.Senses.Source.Light.Prepare";
        public static readonly EntitySystemId LightsCollectionGridSystemId = "Systems.Core.Senses.Source.Light.Collect.Grid";
        public static readonly EntitySystemId LightsCollectionContinuousSystemId = "Systems.Core.Senses.Source.Light.Collect.Continuous";
        public static readonly EntitySystemId LightsComputeSystemId = "Systems.Core.Senses.Source.Light.Compute";
        public static readonly EntitySystemId LightsFinalizedSystemId = "Systems.Core.Senses.Source.Light.Finalize";

        public static readonly EntitySystemId RegisterResistanceEntitiesId = "Core.Entities.Senses.Resistance.Light";
        public static readonly EntitySystemId RegisterResistanceSystem = "Core.Systems.Senses.Resistance.Light.SetUp";
        public static readonly EntitySystemId ExecuteResistanceSystem = "Core.Systems.Senses.Resistance.Light.Run";

        public static readonly EntitySystemId SenseCacheLifecycleId = "Core.Systems.Senses.Cache.Light.Lifecycle";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Source.Light";

        public LightSourceModule()
        {
            Id = ModuleId;

            DeclareDependencies(ModuleDependency.Of(SensoryCacheModule.ModuleId),
                                ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(LightSourceRole).WithImpliedRole(SenseSources.SenseSourceRole).WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);

            ForRole(ResistanceDataProviderRole).WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light")]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : ITimeContext
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterEntities);
            ctx.Register(LightsPreparationSystemId, 50000, RegisterPrepareSystem);
            ctx.Register(LightsComputeSystemId, 58000, RegisterCalculateSystem);
            ctx.Register(LightsFinalizedSystemId, 59000, RegisterCleanUpSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeLightCollectionGrid<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                            IModuleInitializer<TGameContext> initializer,
                                                                            EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(LightsCollectionGridSystemId, 55000, RegisterCollectLightsGridSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeLightCollectionContinuous<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                  IModuleInitializer<TGameContext> initializer,
                                                                                  EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(LightsCollectionGridSystemId, 55000, RegisterCollectLightsContinuousSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light",
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
                o.RegisterSense<VisionSense>();
            }
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light",
                               ConditionalRoles = new[]
                               {
                                   "Role.Core.Senses.Resistance.Light"
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
            
            ctx.Register(SenseCacheLifecycleId, 500, RegisterSenseResistanceCacheLifeCycle<TGameContext, TItemId, VisionSense>);
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateSensePhysics(IServiceResolver resolver)
        {
            var physics = resolver.Resolve<ILightPhysicsConfiguration>();
            return (physics.CreateLightPropagationAlgorithm(), physics.LightPhysics);
        }
    }
}