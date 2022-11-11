using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Text;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

/// <summary>
///    Stores region entries.
///
///    Each entry marks a region (an continuous walkable area) within this zone. Each region then can have
///    multiple region edges connecting the region to neighbouring regions. Each such edge contains a single
///    coordinate inside the neighbouring zone as reference point. 
/// </summary>
public class PathfinderRegionDataView : DefaultPooledBoundedDataView<(TraversableZoneId zone, DirectionalityInformation zoneEdges)>
{
    ushort zoneIdTracker;
    ushort edgeIdTracker;

    public PathfinderRegionDataView(in Rectangle bounds,
                                    long time) : base(in bounds, time)
    {
    }

    public RegionEdgeState State { get; set; }

    public int RegionAge { get; set; }

    public void ClearData()
    {
        Clear();
        State = RegionEdgeState.Dirty;
        zoneIdTracker = 0;
        edgeIdTracker = 0;
    }

    public bool TryGetZoneId(Position2D pos, out TraversableZoneId zone)
    {
        if (TryGet(pos, out var raw))
        {
            zone = raw.zone;
            return true;
        }

        zone = default;
        return false;
    }

    public TraversableZoneId GenerateZoneId()
    {
        if (zoneIdTracker == ushort.MaxValue)
        {
            throw new InvalidOperationException("Local zone-id limit reached");
        }

        zoneIdTracker += 1;
        var retval = new TraversableZoneId(zoneIdTracker);
        return retval;
    }

    public EdgeId GenerateEdgeId()
    {
        edgeIdTracker += 1;
        return new EdgeId(edgeIdTracker);
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
                if ((data.zoneEdges & DirectionalityInformation.UpLeft) != 0)
                {
                    sb.Append("\\");
                }
                else
                {
                    sb.Append(".");
                }

                if ((data.zoneEdges & DirectionalityInformation.Up) != 0)
                {
                    sb.Append("|");
                }
                else
                {
                    sb.Append(".");
                }

                if ((data.zoneEdges & DirectionalityInformation.UpRight) != 0)
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
                if ((data.zoneEdges & DirectionalityInformation.Left) != 0)
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

                if ((data.zoneEdges & DirectionalityInformation.Right) != 0)
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
                if ((data.zoneEdges & DirectionalityInformation.DownLeft) != 0)
                {
                    sb.Append("/");
                }
                else
                {
                    sb.Append(".");
                }

                if ((data.zoneEdges & DirectionalityInformation.Down) != 0)
                {
                    sb.Append("|");
                }
                else
                {
                    sb.Append(".");
                }

                if ((data.zoneEdges & DirectionalityInformation.DownRight) != 0)
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

    public void MarkDirty(RegionEdgeState regionEdgeState)
    {
        if (this.State == RegionEdgeState.Dirty)
        {
            return;
        }

        switch (regionEdgeState)
        {
            case RegionEdgeState.MarkedForRemove:
            {
                this.State = RegionEdgeState.MarkedForRemove;
                return;
            } 
            case RegionEdgeState.PathDirty:
            {
                this.State = RegionEdgeState.PathDirty;
                return;
            }
            default:
            {
                this.State |= regionEdgeState;
                return;
            }
        }
        
    }
}