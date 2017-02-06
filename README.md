# BuildPipeline.Cake

`Pipeline.cake` is a custom [Cake](http://cakebuild.net/) script to build C# projects. Run `.\build.ps1` in the SampleApplication folder for an example.

## What It Does

`Pipeline.cake` performs a set of given steps:

- Clean
- NuGet restore
- Build
- Test (VSTest and OpenCover)
- [Resharper CLI tools](https://www.jetbrains.com/resharper/features/command-line.html) (DupFinder and InspectCode)

All generated reports are saved in the `BuildArtifacts` folder.

## License

[MIT](http://opensource.org/licenses/MIT)
