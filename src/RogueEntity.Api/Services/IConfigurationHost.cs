namespace RogueEntity.Api.Services
{
    public interface IConfigurationHost
    {
        TConfigObject GetConfiguration<TConfigObject>(string sectionName)
            where TConfigObject : new();
    }
}
