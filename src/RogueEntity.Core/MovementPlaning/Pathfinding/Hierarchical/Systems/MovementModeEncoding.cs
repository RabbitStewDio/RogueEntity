using RogueEntity.Api.Utils;
using RogueEntity.Core.Movement;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;

public class MovementModeEncoding
{
    readonly Dictionary<IMovementMode, byte> modes;
    readonly List<IMovementMode> modeList;

    public MovementModeEncoding()
    {
        modeList = new List<IMovementMode>();
        modes = new Dictionary<IMovementMode, byte>();
    }

    public ReadOnlyListWrapper<IMovementMode> ModeList => modeList;
    
    public void Register(IMovementMode m)
    {
        if (modes.Count == 255) throw new ArgumentException();
        
        if (!modes.TryGetValue(m, out _))
        {
            modes[m] = (byte)modes.Count;
            modeList.Add(m);
        }
    }

    public IMovementMode this[byte b] => modeList[b]; 

    public bool TryGetModeIndex(IMovementMode m, out byte idx)
    {
        return modes.TryGetValue(m, out idx);
    }
}