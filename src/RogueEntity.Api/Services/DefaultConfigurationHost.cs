using Microsoft.Extensions.Configuration;
using System;

namespace RogueEntity.Api.Services
{
    public class DefaultConfigurationHost : IConfigurationHost
    {
        readonly IConfiguration config;

        public DefaultConfigurationHost(IConfiguration config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public TConfigObject GetConfiguration<TConfigObject>(string sectionName) where TConfigObject: new()
        {
            var raw = this.config.GetSection(sectionName).Get(typeof(TConfigObject));
            if (raw is TConfigObject conf)
            {
                return conf;
            }

            return new TConfigObject();
        }
    }
}
