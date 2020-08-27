namespace RogueEntity.Core.Meta.Naming
{
    public interface IDisplayName
    {
        string GetIndefiniteFormName(int amount);
        string GetDefiniteFormName(int amount);
    }
}