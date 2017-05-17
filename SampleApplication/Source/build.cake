#load "../../Cake.Mug/Content/build.cake"

#addin nuget:?package=Cake.FileHelpers&version=1.0.4

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = "1.0.0.1";

BuildParameters.Configuration = configuration;
BuildParameters.AddMSBuildProperty("MyCustomProperty", "value1", "value2");

Task("Default")
    .IsDependentOn("Analyze")
    .IsDependentOn("CreatePackages")
    .IsDependentOn("CreateClickOnce")
    .Does(() =>
{
});

Task("InjectVersion")
    .IsDependentOn("Initialize")
    .Does(() =>
{
    Information("Injecting version: {0}", version);
    var versionRegex = @"\d+\.\d+\.\d+\.\d+|\d+\.\d+\.\d+";

    var assemblyInfoFiles = BuildParameters.SolutionDir + "/**/AssemblyInfo.cs";
    var chocoFiles = BuildParameters.ChocolateySpecs + "/**/*.nuspec";
    var nugetFiles = BuildParameters.NuGetSpecs + "/**/*.nuspec";

    ReplaceRegexInFiles(assemblyInfoFiles, versionRegex, version);
    ReplaceRegexInFiles(chocoFiles, versionRegex, version);
    ReplaceRegexInFiles(nugetFiles, versionRegex, version);
});

Task("CreateClickOnce")
    .Does(() =>
{
    MSBuildSettings settings = new MSBuildSettings()
        .SetConfiguration(BuildParameters.Configuration)
        .WithTarget("publish")
        .WithProperty("ApplicationVersion", version);

    MSBuild("SampleClickOnce/SampleClickOnce.csproj", settings);
});

RunTarget(target);
