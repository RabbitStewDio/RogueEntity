namespace RogueEntity.Api.Modules.Helpers
{
    public interface IGlobalSystemDeclaration: ISystemDeclaration
    {
        GlobalSystemRegistrationDelegate SystemRegistration { get; }
    }
}