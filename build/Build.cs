using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Octokit;
using static Nuke.Common.IO.PathConstruction;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
[DotNetVerbosityMapping]
partial class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Pack);

    [Solution]
    readonly Solution Solution;

    [GitRepository]
    readonly GitRepository GitRepository;

    [GitVersion]
    readonly GitVersion GitVersion;

    string SemanticVersion => GitVersion.MajorMinorPatch;
    string ChangelogFile => RootDirectory / "CHANGELOG.md";

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestDirectory => RootDirectory / "tests";
    AbsolutePath OutputDirectory => RootDirectory / "output";
    AbsolutePath ArtifactsDirectory => RootDirectory / "Artifacts";

    IEnumerable<string> SpecificationFiles => GlobFiles(SourceDirectory, "*/*.json");
    IEnumerable<Nuke.Common.ProjectModel.Project> TestProjects => Solution.GetProjects("*.Tests");

    bool GitHubMilestoneClosed(bool mustExist) => GetMilestone().Result?.State.Value == ItemState.Closed;

    async Task<Milestone> GetMilestone()
    {
        var client = new GitHubClient(new ProductHeaderValue(nameof(NukeBuild))) {Credentials = new Credentials(GitHubToken)};
        var milestones = await client.Issue.Milestone.GetAllForRepository(
            GitRepository.GetGitHubOwner(),
            GitRepository.GetGitHubName());
        return milestones.FirstOrDefault(x => x.Title == $"v{SemanticVersion}");
    }
}