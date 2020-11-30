using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Movement
{
    /// <summary>
    ///   An information interface that lists all active movement modes currently employed
    ///   by any actor in the system.
    /// </summary>
    public interface IMovementModeRegistry
    {
        public bool TryGetMode<TMode>(out TMode mode);
        public ReadOnlyListWrapper<IMovementMode> Modes { get; }
        public void ExecuteAsGeneric(IMovementMode mode, IGenericLifter<IMovementMode> lifter);
    }
}