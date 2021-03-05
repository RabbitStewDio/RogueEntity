using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;

namespace RogueEntity.Core.MovementPlaning.Goals
{
    public class GoalModule: ModuleBase
    {
        public const string ModuleId = "Core.MovementPlaning.Goals";
        public static readonly EntityRole GoalMarkerRole = new EntityRole("Role.Core.MovementPlaning.Goals.GoalMarker");
        
        public GoalModule()
        {
            Id = ModuleId;

            RequireRole(GoalMarkerRole);
        }
    }
}
