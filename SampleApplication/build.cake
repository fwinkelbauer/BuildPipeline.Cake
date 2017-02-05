// TODO Use a NuGet package instead when Cake 0.18.0 is released
#load pipeline.cake

var target = Argument("target", "Default");

PipelineSettings.Solution = "SampleApplication.sln";

Task("Default")
    .IsDependentOn("BuildPipeline")
    .Does(() =>
{
});

RunTarget(target);