# Build a solution

With the setup of Nuke you get the possibility to add default tasks to build your project.

This default tasks are for restoring, cleaning and then compiling the project and are basically the tasks you want to run before you then run tests, pack everything into a nuget package or create an actual release.

The "clean" tasks looks like this:

```
Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });
```

The "restore" task looks like this:

```
Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });
```

Finally the compile task looks like this:

```
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
```

As you can see Nuke gives us a fluent api to run our tasks by using `DotNetRestore` or `DotNetBuild`.
It also has fluent apis for different tools like BenchmarkDotNet, Coverlet, Github, Docker or Slack and more.

## GitVersion

When you look at the Compile task you see the usages of GitVersion to set Assembly- and file-version during compiling. GitVersion is a tool which creates automatic version by looking at your repository and based on configuration. This configuration can be added to a GitVersion.yml file.

Have a look at the file to see how this repo uses GitVersion.