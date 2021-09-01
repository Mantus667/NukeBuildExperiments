using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Octokit;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Test);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Github Auth Token for Repo")]
    readonly string GithubRepositoryAuthToken;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath TestResultDirectory => RootDirectory / "test_results";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var project = Solution.GetProject("DummyFramework.UnitTests");
            var projectDirectory = Path.GetDirectoryName(project.Path);
            string testFile = TestResultDirectory / $"unittests.testresults";
            // This is so that the global dotnet is used instead of the one that comes with NUKE
            var dotnetPath = ToolPathResolver.GetPathExecutable("dotnet");

            DotNetTest(s => s
            .SetProjectFile(project.Path.ToString())
            .SetConfiguration(Configuration)
            .EnableNoBuild()
            .EnableNoRestore()
            .SetProcessWorkingDirectory(projectDirectory));
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Produces(ArtifactsDirectory)
        .Executes(() =>
        {
            DotNetPack(c => c
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(GitVersion.NuGetVersionV2)
                .SetOutputDirectory(ArtifactsDirectory)
                .SetNoBuild(true));
        });

    Target CreateRelease => _ => _
        .DependsOn(Pack)
        .Executes(() =>
        {
            Logger.Info("Started creating the release");
            GitHubTasks.GitHubClient = new GitHubClient(new ProductHeaderValue(nameof(NukeBuild)))
            {
                Credentials = new Credentials(GithubRepositoryAuthToken)
            };

            var newRelease = new NewRelease(GitVersion.NuGetVersionV2)
            {
                TargetCommitish = GitVersion.Sha,
                Draft = true,
                Name = $"Release version {GitVersion.SemVer}",
                Prerelease = false,
                Body =
                @$"See release notes in [docs](https://[YourSite]/{GitVersion.Major}.{GitVersion.Minor}/)"
            };

            var createdRelease = GitHubTasks.GitHubClient.Repository.Release.Create(GitRepository.GetGitHubOwner(), GitRepository.GetGitHubName(), newRelease).Result;
            if (ArtifactsDirectory.GlobDirectories("*").Count > 0)
            {
                Logger.Warn(
                  $"Only files on the root of {ArtifactsDirectory} directory will be uploaded as release assets.");
            }

            ArtifactsDirectory.GlobFiles("*").ForEach(p => UploadReleaseAssetToGithub(createdRelease, p));
            var _ = GitHubTasks.GitHubClient.Repository.Release
              .Edit(GitRepository.GetGitHubOwner(), GitRepository.GetGitHubName(), createdRelease.Id, new ReleaseUpdate { Draft = false })
              .Result;
        });

    private void UploadReleaseAssetToGithub(Release release, AbsolutePath asset)
    {
        if (!FileExists(asset))
        {
            return;
        }

        Logger.Info($"Started Uploading {Path.GetFileName(asset)} to the release.");
        var releaseAssetUpload = new ReleaseAssetUpload
        {
            ContentType = "application/x-binary",
            FileName = Path.GetFileName(asset),
            RawData = File.OpenRead(asset)
        };
        var _ = GitHubTasks.GitHubClient.Repository.Release.UploadAsset(release, releaseAssetUpload).Result;
        Logger.Success($"Done Uploading {Path.GetFileName(asset)} to the release.");
    }

}