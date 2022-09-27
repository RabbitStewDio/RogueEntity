using System;
using System.Collections.Generic;
using RogueEntity.Api.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Movement.MovementModes
{
    public class MovementModeRegistry : IMovementModeRegistry
    {
        readonly List<IMovementMode> modes;
        readonly Dictionary<Type, (IMovementMode mode, Action<IGenericLifter<IMovementMode>> lifter)> modesByType;

        public MovementModeRegistry()
        {
            modes = new List<IMovementMode>();
            modesByType = new Dictionary<Type, (IMovementMode mode, Action<IGenericLifter<IMovementMode>> lifter)>();
        }

        public void Register(IMovementMode mode)
        {
            if (modesByType.TryGetValue(mode.GetType(), out _))
            {
                return;
            }

            modesByType[mode.GetType()] = (mode, x => x.Invoke(mode));
            modes.Add(mode);
        }

        public ReadOnlyListWrapper<IMovementMode> Modes
        {
            get { return modes; }
        }

        public bool TryGetMode<TMode>([MaybeNullWhen(false)] out TMode mode)
        {
            if (modesByType.TryGetValue(typeof(TMode), out var m))
            {
                mode = (TMode)m.mode;
                return true;
            }

            mode = default;
            return false;
        }

        public void ExecuteAsGeneric(IMovementMode mode, IGenericLifter<IMovementMode> lifter)
        {
            if (modesByType.TryGetValue(mode.GetType(), out var modeData))
            {
                modeData.lifter(lifter);
            }
        }
    }
}