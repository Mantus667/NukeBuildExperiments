# Creating a Nuget package

Creating a nuget package is actually pretty easy with Nuke. Again we have a fluent api that we can call to create the package.

```
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
```

You can see one difference to the compiling task: We added an additional `.Produces(ArtifactsDirectory)` to make sure to create the directory in which the nuget packages are created.

Again we use GitVersion to set the version of the package based on our repository.