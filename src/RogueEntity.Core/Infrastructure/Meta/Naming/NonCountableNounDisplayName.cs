namespace RogueEntity.Core.Infrastructure.Meta.Naming
{
    public class NonCountableNounDisplayName : IDisplayName
    {
        readonly string singular;

        public NonCountableNounDisplayName(string singular)
        {
            this.singular = singular;
        }

        public string GetIndefiniteFormName(int amount)
        {
            return singular;
        }

        public string GetDefiniteFormName(int amount)
        {
            return singular;
        }
    }
}