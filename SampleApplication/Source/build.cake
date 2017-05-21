#load "../../Cake.Mug/Content/build.cake"

#addin nuget:?package=Cake.FileHelpers&version=1.0.4

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

BuildParameters.Configuration = configuration;
BuildParameters.AddMSBuildProperty("MyCustomProperty", "value1", "value2");

Task("Default")
    .IsDependentOn("Analyze")
    .IsDependentOn("CreatePackages")
    .IsDependentOn("CreateClickOnce")
    .Does(() =>
{
});

Task("CreateClickOnce")
    .IsDependentOn("Initialize")
    .Does(() =>
{
    MSBuildSettings settings = new MSBuildSettings()
        .SetConfiguration(BuildParameters.Configuration)
        .WithTarget("publish")
        .WithProperty("ApplicationVersion", SolutionProperties.AssemblyInfos["SampleClickOnce"].AssemblyVersion + ".0");

    MSBuild("SampleClickOnce/SampleClickOnce.csproj", settings);
});

RunTarget(target);
