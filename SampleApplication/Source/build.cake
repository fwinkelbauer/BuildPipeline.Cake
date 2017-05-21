#load "../../Cake.Mug/Content/build.cake"

var target = Argument("target", "Default");
BuildParameters.Configuration = Argument("configuration", "Release");

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
