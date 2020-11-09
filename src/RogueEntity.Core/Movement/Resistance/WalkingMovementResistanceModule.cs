using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Movement.Resistance
{
    public class WalkingMovementResistanceModule : MovementResistanceModule<WalkingMovement>
    {
        public static readonly EntitySystemId RegisterResistanceEntitiesId = "Core.Entities.Movement.Resistance.Walking";
        public static readonly EntitySystemId RegisterResistanceSystem = "Core.Systems.Movement.Resistance.Walking.SetUp";
        public static readonly EntitySystemId ExecuteResistanceSystem = "Core.Systems.Movement.Resistance.Walking.Run";

        public static readonly string ModuleId = "Core.Movement.Resistance.Walking";

        public WalkingMovementResistanceModule()
        {
            Id = ModuleId;
            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId));
        }
        
        [EntityRoleInitializer("Role.Core.Movement.Resistance.Walking")]
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
        }

    }
}