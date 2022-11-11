using EnTTSharp;
using System;
using System.Collections.Generic;
using RogueEntity.Api.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Movement.MovementModes
{
    public class MovementModeRegistry : IMovementModeRegistry
    {
        interface IMovementModeLift
        {
            public void Invoke(IGenericLifter<IMovementMode> lift);
            public Optional<TResult> Invoke<TResult>(IGenericLifterFunction<IMovementMode> lift);
        }
        
        class MovementModeLift<TMovementMode> : IMovementModeLift
            where TMovementMode : IMovementMode
        {
            public void Invoke(IGenericLifter<IMovementMode> lift) => lift.Invoke<TMovementMode>();
            public Optional<TResult> Invoke<TResult>(IGenericLifterFunction<IMovementMode> lift) => lift.Invoke<TMovementMode, TResult>();
        }
        
        readonly List<IMovementMode> modes;
        readonly Dictionary<Type, (IMovementMode mode, IMovementModeLift lifter)> modesByType;

        public MovementModeRegistry()
        {
            modes = new List<IMovementMode>();
            modesByType = new Dictionary<Type, (IMovementMode mode, IMovementModeLift lifter)>();
        }

        public void Register<TMovementMode>(TMovementMode mode)
            where TMovementMode: IMovementMode
        {
            if (modesByType.TryGetValue(mode.GetType(), out _))
            {
                return;
            }

            modesByType[mode.GetType()] = (mode, new MovementModeLift<TMovementMode>());
            modes.Add(mode);
        }

        public ReadOnlyListWrapper<IMovementMode> Modes
        {
            get { return modes; }
        }

        public bool TryGetMode<TMode>([MaybeNullWhen(false)] out TMode mode) where TMode: IMovementMode
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
                modeData.lifter.Invoke(lifter);
            }
        }
        
        public Optional<TResult> ExecuteAsGeneric<TResult>(IMovementMode mode, IGenericLifterFunction<IMovementMode> lifter)
        {
            if (modesByType.TryGetValue(mode.GetType(), out var modeData))
            {
                return modeData.lifter.Invoke<TResult>(lifter);
            }

            return Optional.Empty();
        }
    }
}