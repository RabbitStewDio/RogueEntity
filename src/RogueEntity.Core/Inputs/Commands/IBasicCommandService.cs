namespace RogueEntity.Core.Inputs.Commands
{
    public interface IBasicCommandService<TActor>
    {
        bool IsActive(TActor actor);
        bool IsValid<TCommand>(TActor actor);
        bool IsValid<TCommand>(TActor actor, TCommand cmd);
        bool TrySubmit<TCommand>(TActor actor, TCommand cmd);
    }
}
