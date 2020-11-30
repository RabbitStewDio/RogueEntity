using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace RogueEntity.Core.Utils.Algorithms
{
    /// <summary>
    ///   8 bytes.
    /// </summary>
    public readonly struct AStarNode
    {
        public static readonly AStarNode Empty = new AStarNode();
        
        public enum NodeState : byte
        {
            [UsedImplicitly] None = 0,
            Open = 1,
            Closed = 2
        }

        // Whether or not the node has been closed
        public readonly NodeState State;

        public readonly Direction DirectionToParent;

        public readonly ushort DistanceFromStart;

        // (Known) distance from start to this node, by shortest known path
        public readonly float AccumulatedCost;


        public AStarNode(NodeState state, float accumulatedCost, Direction directionToParent, ushort distanceFromStart)
        {
            State = state;
            AccumulatedCost = accumulatedCost;
            DirectionToParent = directionToParent;
            DistanceFromStart = distanceFromStart;
        }

        public static AStarNode Start()
        {
            return new AStarNode(NodeState.Open, 0, Direction.None, 0);
        }

        public AStarNode Close()
        {
            return new AStarNode(NodeState.Closed, AccumulatedCost, DirectionToParent, DistanceFromStart);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsClosed()
        {
            return State == NodeState.Closed;
        }
    }
}