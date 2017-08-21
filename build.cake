#tool "nuget:?package=gitreleasemanager"

var user = EnvironmentVariable("GITHUB_USERNAME");
var password = EnvironmentVariable("GITHUB_PASSWORD");
var owner = "fwinkelbauer";
var repository = "Cake.Mug";
var milestone = "0.6.0";

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

Task("CreateGitHubReleaseDraft")
    .Does(() =>
{
    GitReleaseManagerCreate(user, password, owner, repository, new GitReleaseManagerCreateSettings() {
        Milestone = milestone,
        Name = milestone
    });

	GitReleaseManagerExport(user, password, owner, repository, "CHANGELOG.md");
});

Task("PublishGitHubRelease")
    .IsDependentOn("CreatePackages")
    .Does(() =>
{
    foreach (var package in GetFiles(artifactsDir + "/**/*.nupkg"))
    {
        GitReleaseManagerAddAssets(user, password, owner, repository, milestone, package.ToString());
    }

    GitReleaseManagerPublish(user, password, owner, repository, milestone);
    GitReleaseManagerClose(user, password, owner, repository, milestone);
});

Task("Publish")
    .IsDependentOn("PushPackages")
    .IsDependentOn("PublishGitHubRelease")
    .Does(() =>
{
});

RunTarget(target);
