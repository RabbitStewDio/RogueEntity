using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical
{
    public enum EdgeDetectorNode : byte
    {
        [UsedImplicitly] None = 0,
        Open = 1,
        Closed = 2
    }
    
    public readonly struct PathfinderRegionEdgeDetector
    {
        public readonly PathfinderRegionDataView Region;
        readonly PooledDynamicDataView2D<EdgeDetectorNode> visitedNodes;

        void ReconnectEdge(in Rectangle b)
        {
            foreach (var p in b.Contents)
            {
                var r = Region[p.X, p.Y];
                var v = visitedNodes[p.X, p.Y];
                if (v == EdgeDetectorNode.Closed)
                {
                    continue;
                }

                ReconnectEdgesAt(p);
            }
        }

        void ReconnectEdgesAt(Position2D p)
        {
            
        }

        public void ReconnectEdge(EdgeDirection direction)
        {
            var bounds = Region.Bounds;
            switch (direction)
            {
                case EdgeDirection.North:
                {
                    ReconnectEdge(new Rectangle(bounds.X, bounds.Y, bounds.Width, 1));
                    break;
                }
                case EdgeDirection.NorthEast:
                {
                    ReconnectEdge(new Rectangle(bounds.MaxExtentX, bounds.Y, 1, 1));
                    break;
                }
                case EdgeDirection.East:
                {
                    ReconnectEdge(new Rectangle(bounds.MaxExtentX, bounds.Y, 1, bounds.Height));
                    break;
                }
                case EdgeDirection.SouthEast:
                {
                    ReconnectEdge(new Rectangle(bounds.MaxExtentX, bounds.MaxExtentY, 1, 1));
                    break;
                }
                case EdgeDirection.South:
                {
                    ReconnectEdge(new Rectangle(bounds.X, bounds.MaxExtentY, bounds.Width, 1));
                    break;
                }
                case EdgeDirection.SouthWest:
                {
                    ReconnectEdge(new Rectangle(bounds.X, bounds.MaxExtentY, 1, 1));
                    break;
                }
                case EdgeDirection.West:
                {
                    ReconnectEdge(new Rectangle(bounds.X, bounds.Y, 1, bounds.Height));
                    break;
                }
                case EdgeDirection.NorthWest:
                {
                    ReconnectEdge(new Rectangle(bounds.X, bounds.Y, 1, 1));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        void Reconnect(TraversableZoneId traversableZoneId)
        {
            // var start = 
        }
    }
}