namespace RogueEntity.Api.Modules
{
    public interface ISystemDeclaration
    {
        ModuleId DeclaringModule { get; }
        EntitySystemId Id { get; }
        int Priority { get; }
        int InsertionOrder { get; }
    }
}