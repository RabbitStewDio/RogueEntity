using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Utilities.Collections;
using Nuke.GitHub;
using static Nuke.GitHub.GitHubTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.CodeGeneration.CodeGenerator;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.ChangeLog.ChangelogTasks;

partial class Build
{

    Target Clean => _ => _
        .Executes(() =>
        {
            System.Console.WriteLine(ToolPathResolver.ExecutingAssemblyDirectory);
            System.Console.WriteLine(ToolPathResolver.NuGetAssetsConfigFile);
            System.Console.WriteLine(ToolPathResolver.NuGetPackagesConfigFile);
            System.Console.WriteLine(ToolPathResolver.PaketPackagesConfigFile);


            SourceDirectory.GlobDirectories("*/bin", "*/obj").ForEach(DeleteDirectory);
            TestDirectory.GlobDirectories("*/bin", "*/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });


    Target Generate => _ => _
                            .OnlyWhenDynamic(() => SpecificationFiles.Any())
                            .Executes(() =>
                            {
                                string GetNamespace(string specificationFile)
                                    => Solution.Projects.Single(x => IsDescendantPath(x.Directory, specificationFile)).Name;

                                foreach (var s in SpecificationFiles)
                                {
                                    GenerateCode(
                                        s,
                                        outputFileProvider: x => x.DefaultOutputFile,
                                        namespaceProvider: x => GetNamespace(x.SpecificationFile),
                                        sourceFileProvider: x => GitRepository.GetGitHubBrowseUrl(x.SpecificationFile));
                                }
                            });

    Target Restore => _ => _
                           .After(Clean)
                           .Executes(() =>
                           {
                               DotNetRestore(s => s
                                                  .SetProjectFile(Solution)
                                                  .SetProperty("ReplacePackageReferences", false));
                           });

    Target Compile => _ => _
                           .DependsOn(Restore, Generate)
                           .Executes(() =>
                           {
                               DotNetBuild(s => s
                                                .SetProjectFile(Solution)
                                                .EnableNoRestore()
                                                .SetConfiguration(Configuration)
                                                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                                                .SetFileVersion(GitVersion.AssemblySemFileVer)
                                                .SetInformationalVersion(GitVersion.InformationalVersion)
                                                .SetProperty("ReplacePackageReferences", false));
                           });

    Target Test => _ => _
                        .DependsOn(Compile)
                        .OnlyWhenDynamic(() => TestProjects.Any())
                        .Executes(() =>
                        {
                            DotNetTest(s => s
                                            .SetConfiguration(Configuration)
                                            .EnableNoBuild()
                                            .SetLogger("trx")
                                            .SetResultsDirectory(OutputDirectory)
                                            .CombineWith(TestProjects, (cs, v) => cs
                                                             .SetProjectFile(v)));
                        });


    Target Changelog => _ => _
                             .After(Test)
                             .Before(Pack)
                             .OnlyWhenDynamic(() =>
                                                  GitRepository.IsOnMasterBranch() ||
                                                  GitRepository.IsOnReleaseBranch() ||
                                                  GitRepository.IsOnHotfixBranch())
                             .Executes(() =>
                             {
                                 FinalizeChangelog(ChangelogFile, SemanticVersion, GitRepository);
                                 Git($"add {ChangelogFile}");
                                 Git($"commit -m \"Finalize {Path.GetFileName(ChangelogFile)} for v{SemanticVersion}\"");
                             });

    Target Pack => _ => _
                        .DependsOn(Compile)
                        .Executes(() =>
                        {
                            DotNetPack(s => s
                                            .SetProject(Solution)
                                            .EnableNoBuild()
                                            .SetConfiguration(Configuration)
                                            .EnableIncludeSymbols()
                                            .SetSymbolPackageFormat(DotNetSymbolPackageFormat.snupkg)
                                            .SetOutputDirectory(ArtifactsDirectory)
                                            .SetVersion(GitVersion.NuGetVersionV2)
                                            .SetPackageReleaseNotes(GetNuGetReleaseNotes(ChangelogFile, GitRepository)));
                        });

    Target Publish => _ => _
                           .DependsOn(Changelog)
                           .DependsOn(Pack)
                           .Requires(() => ApiKey)
                           // .Requires(() => GitHubToken)
                           .Requires(() => GitHasCleanWorkingCopy())
                           // .Requires(() => GitHubMilestoneClosed(mustExist: false))
                           .Requires(() => Configuration.Equals(Configuration.Release))
                           // .Requires(() => GitRepository.IsOnMasterBranch() ||
                           //                 GitRepository.IsOnDevelopBranch() ||
                           //                 GitRepository.IsOnReleaseBranch() ||
                           //                 GitRepository.IsOnHotfixBranch())
                           .Executes(async () =>
                           {
                               DotNetNuGetPush(s => s
                                                    .SetSource(Source)
                                                    .SetSymbolSource(SymbolSource)
                                                    .SetApiKey(ApiKey)
                                                    .CombineWith(OutputDirectory.GlobFiles("*.nupkg").NotEmpty(), (cs, v) => cs
                                                                     .SetTargetPath(v)),
                                               degreeOfParallelism: 5,
                                               completeOnFailure: true);

                               Git($"tag {SemanticVersion}");
                               Git($"push origin {GitRepository.Branch} {SemanticVersion}");

                               return;

                               await PublishRelease(s => s
                                                         .SetToken(GitHubToken)
                                                         .SetRepositoryOwner(GitRepository.GetGitHubOwner())
                                                         .SetRepositoryName(GitRepository.GetGitHubName())
                                                         .SetCommitSha(GitRepository.Branch)
                                                         .SetTag($"{SemanticVersion}")
                                                         .SetName($"v{SemanticVersion}")
                                                         .SetReleaseNotes(GetNuGetReleaseNotes(ChangelogFile, GitRepository)));
                           });

}