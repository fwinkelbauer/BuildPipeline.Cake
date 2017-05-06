#load "../../Cake.BuildUtil/Content/build.cake"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

BuildParameters.Configuration = configuration;
BuildParameters.Version = "1.0.0.1";
BuildParameters.AddMSBuildProperty("MyCustomProperty", "value1", "value2");

Task("Default")
    .IsDependentOn("CreatePackages")
    .IsDependentOn("CreateClickOnce")
    .Does(() =>
{
});

Task("CreateClickOnce")
    .Does(() =>
{
    MSBuildSettings settings = new MSBuildSettings()
        .SetConfiguration(BuildParameters.Configuration)
        .WithTarget("publish")
        .WithProperty("ApplicationVersion", BuildParameters.Version);

    MSBuild("SampleClickOnce/SampleClickOnce.csproj", settings);
});

RunTarget(target);
