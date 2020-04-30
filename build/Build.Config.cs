using System;
using Nuke.Common;

partial class Build
{
    const string NUGET_PACKAGESOURCE = "https://api.nuget.org/v3/index.json";
    const string NUGET_SYMBOLSOURCE = "https://nuget.smbsrc.net/";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = DefaultConfig();

    [Parameter("NuGet API URL (Uses NUGET_SOURCE environment variable if defined, defaults to public NuGet API v3).")] 
    readonly string Source = Environment.GetEnvironmentVariable("NUGET_SOURCE") ?? NUGET_PACKAGESOURCE;
    [Parameter("NuGet Symbol Server URL.")] 
    readonly string SymbolSource = Environment.GetEnvironmentVariable("NUGET_SYMBOLS_SOURCE") ?? NUGET_SYMBOLSOURCE;

    [Parameter("ApiKey for the specified source. Defaults to environment variable NUGET_API_KEY")] 
    readonly string ApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY");

    [Parameter("GitHub Personal API Token")] 
    readonly string GitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

    static Configuration DefaultConfig()
    {
        return IsLocalBuild ? Configuration.Debug : Configuration.Release;
    }
}