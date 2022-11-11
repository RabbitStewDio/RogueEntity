using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.MovementPlaning.Goals
{
    public class GoalModule: ModuleBase
    {
        public const string ModuleId = "Core.MovementPlaning.Goals";
        public static readonly EntityRole GoalMarkerRole = new EntityRole("Role.Core.MovementPlaning.Goals.GoalMarker");
        public static EntityRole GetGoalMarkerInstanceRole<TGoal>()
            where TGoal: IGoal
            => new EntityRole("Role.Core.MovementPlaning.Goals.GoalMarker." + typeof(TGoal).Name);
        
        public static readonly EntitySystemId GoalMarkerComponentsId = "Entities.Core.GoalMarker";

        public GoalModule()
        {
            Id = ModuleId;

            RequireRole(GoalMarkerRole).WithImpliedRole(PositionModule.PositionQueryRole);
        }
        
        [EntityRoleInitializer("Role.Core.MovementPlaning.Goals.GoalMarker")]
        protected void InitializePlayerObserverRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                             IModuleInitializer initializer,
                                                             EntityRole r)
            where TItemId : struct, IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
           //  entityContext.Register(GoalMarkerComponentsId, -20_000, RegisterGoalComponents);
        }


    }
}
