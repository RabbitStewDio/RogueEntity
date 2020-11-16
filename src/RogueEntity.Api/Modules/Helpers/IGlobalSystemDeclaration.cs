namespace RogueEntity.Api.Modules.Helpers
{
    public interface IGlobalSystemDeclaration<TGameContext>: ISystemDeclaration
    {
        GlobalSystemRegistrationDelegate<TGameContext> SystemRegistration { get; }
    }
}