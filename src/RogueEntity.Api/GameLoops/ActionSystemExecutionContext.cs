namespace RogueEntity.Api.GameLoops
{
    public enum ActionSystemExecutionContext
    {
        Initialization = 0,
        PrepareFixedStep = 1,
        FixedStep = 2,
        LateFixedStep = 3,
        VariableStep = 4,
        LateVariableStep = 5,
        ShutDown = -1,
    }
}
