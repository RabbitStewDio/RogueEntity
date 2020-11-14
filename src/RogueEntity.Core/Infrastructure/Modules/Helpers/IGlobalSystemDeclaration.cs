namespace RogueEntity.Core.Infrastructure.Modules.Helpers
{
    public interface IGlobalSystemDeclaration<TGameContext>: ISystemDeclaration
    {
        GlobalSystemRegistrationDelegate<TGameContext> SystemRegistration { get; }
    }
}