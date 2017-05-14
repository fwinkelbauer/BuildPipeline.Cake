# Cake.Mug

This is a custom [Cake](http://cakebuild.net/) script to build C# projects. Read/Run `.\build.ps1` in the `SampleApplication\Source` folder for an example.

## What It Does

Cake.Mug performs a set of given steps:

- Initialize (Setting a few variables which are used in the build process)
- Info (Printing some build information)
- Clean
- NuGet restore
- Version injecting (AssemblyInfo.cs, *.nuspec)
- MSBuild
  - Building the solution
  - Building WiX projects
- VSMetrics (Using Visual Studio's Powertool `metrics.exe`)
- VSTest + OpenCover
- [Resharper CLI tools](https://www.jetbrains.com/resharper/features/command-line.html) (DupFinder and InspectCode)

All generated artifacts are saved in the `BuildArtifacts` folder.

## License

[MIT](http://opensource.org/licenses/MIT)
