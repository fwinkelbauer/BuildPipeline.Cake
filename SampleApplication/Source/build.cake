#load "../../Cake.Mug.Tools/Content/tools.cake"
#load "../../Cake.Mug/Content/configuration.cake"
#load "../../Cake.Mug/Content/build.cake"

var target = Argument("target", "Default");
BuildParameters.Configuration = Argument("configuration", "Release");

Task("Default")
    .IsDependentOn("Analyze")
    .IsDependentOn("CreatePackages")
    .Does(() =>
{
});

Task("PublishClickOnce")
    .IsDependentOn("Initialize")
    .Does(() =>
{
    MSBuildSettings settings = new MSBuildSettings()
        .SetConfiguration(BuildParameters.Configuration)
        .WithTarget("publish");

    MSBuild("SampleClickOnce/SampleClickOnce.csproj", settings);
});

RunTarget(target);
