using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Sensing.Sources.Light;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    public class VisionSenseModule : SenseReceptorModuleBase<VisionSense, VisionSense, LightSourceDefinition>
    {
        public const string ModuleId = "Core.Sense.Receptor.Vision";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Receptor.Vision";

        public static readonly EntitySystemId ReceptorPreparationSystemId = "Systems.Core.Senses.Receptor.Vision.Prepare";
        public static readonly EntitySystemId ReceptorCollectionGridSystemId = "Systems.Core.Senses.Receptor.Vision.CollectReceptors.Grid";
        public static readonly EntitySystemId ReceptorCollectionContinuousSystemId = "Systems.Core.Senses.Receptor.Vision.CollectReceptors.Continuous";
        public static readonly EntitySystemId ReceptorComputeFoVSystemId = "Systems.Core.Senses.Receptor.Vision.ComputeFieldOfView";
        public static readonly EntitySystemId SenseSourceCollectionSystemId = "Systems.Core.Senses.Receptor.Vision.CollectSources.Continuous";
        public static readonly EntitySystemId ReceptorComputeSystemId = "Systems.Core.Senses.Receptor.Vision.Compute";
        public static readonly EntitySystemId ReceptorFinalizeSystemId = "Systems.Core.Senses.Receptor.Vision.Finalize";

        public static readonly EntityRole SenseReceptorActorRole = new EntityRole("Role.Core.Senses.Receptor.Vision.ActorRole");

        public VisionSenseModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Receptors";
            Description = "Provides items and actors with a field of view.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId),
                                ModuleDependency.Of(LightSourceModule.ModuleId));

            RequireRole(SenseReceptorActorRole).WithImpliedRole(SenseReceptors.SenseReceptorRole).WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Vision.ActorRole")]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterEntities);
            ctx.Register(ReceptorPreparationSystemId, 50000, RegisterPrepareSystem);
            ctx.Register(ReceptorComputeFoVSystemId, 56000, RegisterComputeReceptorFieldOfView);
            ctx.Register(ReceptorComputeSystemId, 58500, RegisterCalculateOmniDirectionalSystem);
            ctx.Register(ReceptorFinalizeSystemId, 59500, RegisterFinalizeSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Vision.ActorRole",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeCollectReceptorsGrid<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                             IModuleInitializer<TGameContext> initializer,
                                                                             EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionGridSystemId, 55500, RegisterCollectReceptorsGridSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Vision.ActorRole",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeCollectReceptorsContinuous<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                   IModuleInitializer<TGameContext> initializer,
                                                                                   EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionContinuousSystemId, 55500, RegisterCollectReceptorsContinuousSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light")]
        protected void InitializeSenseCollection<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                        IModuleInitializer<TGameContext> initializer,
                                                                        EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SenseSourceCollectionSystemId, 57500, RegisterCollectSenseSourcesSystem);
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out IVisionSenseReceptorPhysicsConfiguration physics))
            {
                var physicsConfig = serviceResolver.Resolve<ILightPhysicsConfiguration>();
                if (!serviceResolver.TryResolve(out ShadowPropagationResistanceDataSource ds))
                {
                    ds = new ShadowPropagationResistanceDataSource();
                }

                physics = new VisionSenseReceptorPhysicsConfiguration(physicsConfig, ds);
            }

            return (physics.CreateVisionSensorPropagationAlgorithm(), physics.VisionPhysics);
        }
    }
}