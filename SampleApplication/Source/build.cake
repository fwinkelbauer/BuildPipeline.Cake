#load "../../Cake.BuildUtil/Content/build.cake"

var target = Argument("target", "Default");

BuildParameters.Version = "1.0.0.1";
BuildParameters.ClickOnceProjects = new FilePath[] { "SampleClickOnce/SampleClickOnce.csproj" };
BuildParameters.AddProperty("Release", new MSBuildProperty("ApplicationVersion", BuildParameters.Version));

Task("Default")
    .IsDependentOn("CreatePackages")
    .Does(() =>
{
});

RunTarget(target);
