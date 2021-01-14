using RogueEntity.Core.Players;

namespace RogueEntity.Core.Inputs.Commands
{
    /// <summary>
    ///  A global service interface that encapsulates the actual dirty work of handling command state.
    /// </summary>
    public interface IPlayerCommandService<TActorId>
    {
        bool TryGetCommandQueue(PlayerTag player, out ICommandReceiver<TActorId> s);
    }

}
