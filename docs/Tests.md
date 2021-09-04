# Running tests

Nuke has support for different UnitTesting frameworks.
In this repository we have one test in our testing project which uses xunit.

Again we can use the fluent api to run our tests:

```
Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var project = Solution.GetProject("DummyFramework.UnitTests");
            var projectDirectory = Path.GetDirectoryName(project.Path);

            DotNetTest(s => s
            .SetProjectFile(project.Path.ToString())
            .SetConfiguration(Configuration)
            .EnableNoBuild()
            .EnableNoRestore()
            .SetProcessWorkingDirectory(projectDirectory));
        });
```