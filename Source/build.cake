#load pipeline.cake

var target = Argument("target", "Default");

PipelineSettings.Solution = "SampleApplication.sln";
PipelineSettings.VsMetricsFiles = new FilePath[] {
    "SampleApplication/bin/" + PipelineSettings.Configuration + "/SampleApplication.exe",
    "SampleApplication.Tests/bin/" + PipelineSettings.Configuration + "/SampleApplication.Tests.dll"
};

Setup(context => {
    context.Tools.RegisterFile("C:/Program Files (x86)/Microsoft Visual Studio 14.0/Team Tools/Static Analysis Tools/FxCop/metrics.exe");
});

Task("Default")
    .IsDependentOn("Test")
    .IsDependentOn("Metrics")
    .IsDependentOn("InspectCode")
    .IsDependentOn("DupFinder")
    .Does(() =>
{
});

RunTarget(target);