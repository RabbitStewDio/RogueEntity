namespace RogueEntity.Api.Services
{
    public static class ConfigurationHostExtensions
    {
        public static TConfigObject GetConfiguration<TConfigObject>(this IConfigurationHost host)
            where TConfigObject : new()
        {
            return host.GetConfiguration<TConfigObject>(typeof(TConfigObject).Name);
        }
    }
}
