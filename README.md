# Cake.Mug

Cake.Mug is a configurable [Cake](http://cakebuild.net/) script which can be used to build, analyze and pack (NuGet and Chocolatey) C# projects.

NuGet packages are provided [here](https://www.nuget.org/packages/Cake.Mug/) and [here](https://www.nuget.org/packages/Cake.Mug.Tools/).

## Usage

A very simplistic `build.cake` script that utilizes Cake.Mug could look like this:

```csharp
// Load all tools that Cake.Mug needs
#load "nuget:?package=cake.mug.tools"
// Load Cake.Mug
#load "nuget:?package=cake.mug"

var target = Argument("target", "Default");
// Set the configuration option for Cake.Mug's build process
BuildParameters.Configuration = Argument("configuration", "Release");

// Set configuration for using Chocolatey pack and push operations (or delete if not need)
PackageParameters.ChocolateySpecs.Add("PATH TO NUSPEC");
PackageParameters.ChocolateyPushSource = "YOUR SOURCE HERE";
PackageParameters.ChocolateyPushApiKey = "YOUR API KEY HERE";

// Set configuration for using NuGet pack and push operations (or delete if not need)
PackageParameters.NuGetSpecs.Add("PATH TO NUSPEC");
PackageParameters.NuGetPushSource = "YOUR SOURCE HERE";
PackageParameters.NuGetPushApiKey = "YOUR API KEY HERE";

// Call the Cake.Mug tasks "Analyze" and "CreatePackages"
Task("Default")
    .IsDependentOn("Analyze")
    .IsDependentOn("CreatePackages")
    .Does(() =>
{
});

// Call the Cake.Mug tasks "Analyze" and "PushPackages"
Task("Publish")
    .IsDependentOn("Analyze")
    .IsDependentOn("PushPackages")
    .Does(() =>
{
});

RunTarget(target);

```

Run `.\build.ps1` in the `SampleApplication\Source` folder for an example. All output can be found in `SampleApplication\BuildArtifacts`.

## Tools

A list of all tools which are used by Cake.Mug can be found [here](Cake.Mug.Tools/Content/tools.cake). You can skip the load operation for the Cake.Mug.Tools package if all specified tools are reachable through the `PATH` environment variable. This can for example be achieved by installing all tools through [Chocolatey](http://chocolatey.org/).

## Conventions / Configuration

Cake.Mug can be configured through several parameters. All configuration has to be done before the `Initialize` task is called. The basic conventions used by Cake.Mug are specified in [configuration.cake](Cake.Mug/Content/configuration.cake), e.g.:

- Cake.Mug expects a directory structure such as `MyProject/Source/MyProject.sln`
- Your `build.cake` file should be placed next to your `.sln` file
- All build artifacts will be put in `MyProject/BuildArtifacts`
- The default run configuration is `Release`
- MSTest projects should contain the word "Tests" in their name

## Tasks

This section describes all tasks specified in Cake.Mug.

### Initialize

Initializes all parameters that will be used later on in the build process. By default, Cake.Mug searches for a `.sln` file in the current working directory. You can either specify a solution directly (e.g. `BuildParameters.Solution = "MyApplication.sln"`), or you can specify the directory instead (e.g. `BuildParameters.SolutionDir = "./Source"`). It is sufficient to use one of these options as the `Initialize` task will set the other parameter on its own.

### Info

**Depends on:** `Initialize`

Prints relevant information such as the used solution (e.g. `ConsoleApplication1.sln`) or the run configuration (e.g. `Release`).

### Clean

**Depends on:** `Info`

Performs three actions:

- Cleans the build artifacts folder
- Cleans the `TestResults` folder used by VSTest
- Cleans the solution using MSBuild

### Restore

**Depends on:** `Clean`

Calls `nuget restore` using the solution file.

### Build

**Depends on:** `Restore`

Builds the solution using MSBuild and copies all output to the `BuildArtifacts` folder.

### VSTest

**Depends on:** `Build`

Runs VSTest and OpenCover on all projects which match a whitelist in the solution. All reports are put into the `BuildArtifacts` folder.

### VSMetrics

**Depends on:** `Build`

Runs the Visual Studio powertool `metrics.exe` on all project output (`.exe` and `.dll` files) found in the solution. The report is put into the `BuildArtifacts` folder.

### DupFinder

**Depends on:** `Info`

Runs the [ReSharper tool](https://www.jetbrains.com/resharper/features/command-line.html) `dupfinder.exe` on the solution and creates an HTML report in the `BuildArtifacts` folder.

### InspectCode

**Depends on:** `Info`

Runs the [ReSharper tool](https://www.jetbrains.com/resharper/features/command-line.html) `inspectcode.exe` on the solution and creates an HTML report in the `BuildArtifacts` folder.

### Analyze

**Depends on:** `VSTest`, `VSMetrics`, `DupFinder`, `InspectCode`

A wrapper task which can be used to run all analytical tasks.

### CreateChocolateyPackages

**Depends on:** `Build`

Builds Chocolatey packages based on `.nuspec` files. All packages can be found in the `BuildArtifacts` folder.

### CreateNuGetPackages

**Depends on:** `Build`

Builds NuGet packages based on `.nuspec` files. All packages can be found in the `BuildArtifacts` folder.

### CreatePackages

**Depends on:** `CreateChocolateyPackages`, `CreateNuGetPackages`

A wrapper task which can be used to run all create package tasks.

### PushChocolateyPackages

**Depends on:** `CreateChocolateyPackages`

Pushes all created Chocolatey packages to a feed.

### PushNuGetPackages

**Depends on:** `CreateNuGetPackages`

Pushes all created NuGet packages to a feed.

### PushPackages

**Depends on:** `PushChocolateyPackages`, `PushNuGetPackages`

A wrapper task which can be used to run all push package tasks.

## License

[MIT](http://opensource.org/licenses/MIT)
