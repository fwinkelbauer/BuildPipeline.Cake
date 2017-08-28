var target = Argument("target", "Default");
var artifactsDir = new DirectoryPath("BuildArtifacts");

Task("Default")
    .IsDependentOn("CreatePackages")
    .Does(() =>
{
});

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
});

Task("CreatePackages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetPack("Cake.Mug/Cake.Mug.nuspec", new NuGetPackSettings() { OutputDirectory = artifactsDir });
    NuGetPack("Cake.Mug.Tools/Cake.Mug.Tools.nuspec", new NuGetPackSettings() { OutputDirectory = artifactsDir });
});

Task("PushPackages")
    .IsDependentOn("CreatePackages")
    .Does(() =>
{
    NuGetPush(GetFiles(artifactsDir + "/**/*.nupkg"), new NuGetPushSettings() { Source = "https://www.nuget.org/api/v2/package" });
});

Task("Publish")
    .IsDependentOn("PushPackages")
    .Does(() =>
{
});

RunTarget(target);
