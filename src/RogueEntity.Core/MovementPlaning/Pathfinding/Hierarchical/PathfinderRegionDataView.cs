using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical
{
    /// <summary>
    ///    Stores region entries. Each entry marks a region (an continuous walkable area) within
    ///    this zone. Each region then can have multiple region edges connecting the region to
    ///    neighbouring regions. 
    /// </summary>
    public class PathfinderRegionDataView : DefaultPooledBoundedDataView<(TraversableZoneId zone, DirectionalityInformation edgeConnections)>
    {
        readonly ObjectPool<List<PathfinderRegionEdge>> regionListPool;
        readonly Dictionary<TraversableZoneId, Position2D> zoneFirstOccurence;
        readonly Dictionary<TraversableZoneId, List<PathfinderRegionEdge>> activeEdges;
        ushort zoneIdTracker;

        public PathfinderRegionDataView(ObjectPool<List<PathfinderRegionEdge>> regionListPool,
                                        in Rectangle bounds,
                                        long time) : base(in bounds, time)
        {
            this.regionListPool = regionListPool ?? throw new ArgumentNullException(nameof(regionListPool));
            this.activeEdges = new Dictionary<TraversableZoneId, List<PathfinderRegionEdge>>();
            this.zoneFirstOccurence = new Dictionary<TraversableZoneId, Position2D>();
        }

        public void AddEdge(PathfinderRegionEdge e)
        {
            if (!activeEdges.TryGetValue(e.OwnerId, out var edges))
            {
                edges = regionListPool.Get();
                activeEdges[e.OwnerId] = edges;
            }

            edges.Add(e);
        }
        
        public bool IsDirty { get; set; }

        public void ClearData()
        {
            Clear();
            IsDirty = false;
            foreach (var e in activeEdges.Values)
            {
                regionListPool.Return(e);
            }

            activeEdges.Clear();
            zoneIdTracker = 0;
        }

        public TraversableZoneId GenerateZoneId(Position2D position2D)
        {
            if (zoneIdTracker == ushort.MaxValue)
            {
                throw new InvalidOperationException("Local zone-id limit reached");
            }

            zoneIdTracker += 1;
            var retval = new TraversableZoneId(zoneIdTracker);
            zoneFirstOccurence[retval] = position2D;
            return retval;
        }

        public bool TryGetFirstOccurence(TraversableZoneId id, out Position2D pos)
        {
            return zoneFirstOccurence.TryGetValue(id, out pos);
        }

        public string PrintEdges()
        {
            var yMin = Bounds.Y;
            var yMax = Bounds.MaxExtentY;
            var xMin = Bounds.X;
            var xMax = Bounds.MaxExtentX;
            StringBuilder sb = new StringBuilder();

            for (var y = yMin; y <= yMax; y += 1)
            {
                for (var x = xMin; x <= xMax; x += 1)
                {
                    var data = this[x, y];
                    if ((data.edgeConnections & DirectionalityInformation.UpLeft) != 0)
                    {
                        sb.Append("\\");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    if ((data.edgeConnections & DirectionalityInformation.Up) != 0)
                    {
                        sb.Append("|");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    if ((data.edgeConnections & DirectionalityInformation.UpRight) != 0)
                    {
                        sb.Append("/");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    sb.Append(" ");
                }

                sb.AppendLine();

                for (var x = xMin; x <= xMax; x += 1)
                {
                    var data = this[x, y];
                    if ((data.edgeConnections & DirectionalityInformation.Left) != 0)
                    {
                        sb.Append("-");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    if (data.zone.Id == 0)
                    {
                        sb.Append("_");
                    }
                    else if (data.zone.Id < 26)
                    {
                        sb.Append((char)(data.zone.Id - 1 + 'a'));
                    }
                    else if (data.zone.Id < 52)

                    {
                        sb.Append((char)((data.zone.Id - 1 - 26) + 'A'));
                    }
                    else
                    {
                        sb.Append((char)((data.zone.Id) - 51 + '\u4800'));
                    }

                    if ((data.edgeConnections & DirectionalityInformation.Right) != 0)
                    {
                        sb.Append("-");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    sb.Append(" ");
                }

                sb.AppendLine();

                for (var x = xMin; x <= xMax; x += 1)
                {
                    var data = this[x, y];
                    if ((data.edgeConnections & DirectionalityInformation.DownLeft) != 0)
                    {
                        sb.Append("/");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    if ((data.edgeConnections & DirectionalityInformation.Down) != 0)
                    {
                        sb.Append("|");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    if ((data.edgeConnections & DirectionalityInformation.DownRight) != 0)
                    {
                        sb.Append("\\");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    sb.Append(" ");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}