// TODO Use a NuGet package instead when Cake 0.18.0 is released
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
    .IsDependentOn("BuildPipeline")
    .Does(() =>
{
});

RunTarget(target);