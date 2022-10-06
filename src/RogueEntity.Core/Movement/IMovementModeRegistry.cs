using RogueEntity.Api.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Movement
{
    /// <summary>
    ///   An information interface that lists all active movement modes currently employed
    ///   by any actor in the system.
    /// </summary>
    public interface IMovementModeRegistry
    {
        public bool TryGetMode<TMode>([MaybeNullWhen(false)] out TMode mode)
            where TMode: IMovementMode;
        public ReadOnlyListWrapper<IMovementMode> Modes { get; }
        public void ExecuteAsGeneric(IMovementMode mode, IGenericLifter<IMovementMode> lifter);
    }
}