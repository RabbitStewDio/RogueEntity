using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.SpatialQueries;

namespace RogueEntity.Core.MovementPlaning.Goals
{
    public class EntityGoalTargetSource3D<TItemId, TGoal> : IGoalFinderTargetSource
        where TItemId : struct, IEntityKey
        where TGoal: IGoal
    {
        readonly BufferList<SpatialQueryResult<TItemId, GoalMarker<TGoal>>> buffer;
        readonly ISpatialQuery<TItemId, GoalMarker<TGoal>> query;

        public EntityGoalTargetSource3D(ISpatialQuery<TItemId, GoalMarker<TGoal>> query)
        {
            this.query = query;
            this.buffer = new BufferList<SpatialQueryResult<TItemId, GoalMarker<TGoal>>>();
        }

        public void Dispose()
        {
        }

        public GoalSet CollectGoals(in Position origin, float range, DistanceCalculation dc, GoalSet receiver)
        {
            var receiverBuffer = GoalSet.PrepareBuffer(receiver);
            
            query.QuerySphere(origin, range, dc, buffer);
            for (var i = 0; i < buffer.Count; i++)
            {
                var c = buffer[i];
                receiverBuffer.Add(c.Position, c.Component.Strength);
            }
            
            return receiverBuffer;
        }
    }
}
