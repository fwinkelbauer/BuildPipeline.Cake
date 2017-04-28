#tool nuget:?package=JetBrains.ReSharper.CommandLineTools&version=2016.3.20161223.160402
#tool nuget:?package=OpenCover&version=4.6.519
#tool nuget:?package=ReportGenerator&version=2.4.5
#tool nuget:?package=ReportUnit&version=1.2.1
#tool nuget:?package=ReSharperReports&version=0.4.0

#load "../../Cake.BuildUtil/Content/build.cake"

var target = Argument("target", "Default");

BuildParameters.ClickOnceProjects = new FilePath[] { "SampleClickOnce/SampleClickOnce.csproj" };
BuildParameters.AddProperty("Release", new MSBuildProperty("ApplicationRevision", "6"));

Setup(context => {
    context.Tools.RegisterFile("C:/Program Files (x86)/Microsoft Visual Studio 14.0/Team Tools/Static Analysis Tools/FxCop/metrics.exe");
});

Task("Default")
    .IsDependentOn("Create-Packages")
    .IsDependentOn("VSMetrics")
    .IsDependentOn("InspectCode")
    .IsDependentOn("DupFinder")
    .Does(() =>
{
});

RunTarget(target);
