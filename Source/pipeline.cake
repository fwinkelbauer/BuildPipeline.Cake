#addin nuget:?package=Cake.ReSharperReports&version=0.6.0
#addin nuget:?package=Cake.VsMetrics&version=0.1.0

public static class BuildArtifactSettings
{
    static BuildArtifactSettings()
    {
        TestResultsDir = new DirectoryPath("TestResults");
        ArtifactsDir = new DirectoryPath("../BuildArtifacts");
        OpenCoverDir = new DirectoryPath(ArtifactsDir + "/OpenCover");
        OpenCoverXml = new FilePath(OpenCoverDir + "/openCover.xml");
        VsTestDir = new DirectoryPath(ArtifactsDir + "/VSTest");
        VsMetricsDir = new DirectoryPath(ArtifactsDir + "/Metrics");
        VsMetricsXml = new FilePath(VsMetricsDir + "/metrics.xml");
        DupFinderDir = new DirectoryPath(ArtifactsDir + "/DupFinder");
        DupFinderXml = new FilePath(DupFinderDir + "/dupFinder.xml");
        DupFinderHtml = new FilePath(DupFinderDir + "/dupFinder.html");
        InspectCodeDir = new DirectoryPath(ArtifactsDir + "/InspectCode");
        InspectCodeXml = new FilePath(InspectCodeDir + "/inspectCode.xml");
        InspectCodeHtml = new FilePath(InspectCodeDir + "/inspectCode.html");
    }

    public static FilePath Solution { get; set; }
    public static DirectoryPath TestResultsDir { get; set; }
    public static DirectoryPath ArtifactsDir { get; set; }
    public static DirectoryPath OpenCoverDir { get; set; }
    public static FilePath OpenCoverXml { get; set; }
    public static DirectoryPath VsTestDir { get; set; }
    public static DirectoryPath VsMetricsDir { get; set; }
    public static FilePath VsMetricsXml { get; set; }
    public static DirectoryPath DupFinderDir { get; set; }
    public static FilePath DupFinderXml { get; set; }
    public static FilePath DupFinderHtml { get; set; }
    public static DirectoryPath InspectCodeDir { get; set; }
    public static FilePath InspectCodeXml { get; set; }
    public static FilePath InspectCodeHtml { get; set; }
}

public static class PipelineSettings
{
    static PipelineSettings()
    {
        DoTreatWarningsAsErrors = true;
        Configuration = "Release";
        ToolVersion = MSBuildToolVersion.Default;
        Platform = MSBuildPlatform.Automatic;
        TestDllWhitelist = "*.Tests*.dll";
        OpenCoverFilter = "+[*]* -[*Test*]*";
        OpenCoverExcludeByFile = "*/*Designer.cs;*/*.g.cs;*/*.g.i.cs";
        DupFinderExcludePattern = new string[] {};
    }

    public static bool DoTreatWarningsAsErrors { get; set; }
    public static string Configuration { get; set; }
    public static MSBuildToolVersion ToolVersion { get; set; }
    public static MSBuildPlatform Platform { get; set; }
    public static Dictionary<string, string[]> Properties { get; private set; }
    public static string TestDllWhitelist { get; set; }
    public static string OpenCoverFilter { get; set; }
    public static string OpenCoverExcludeByFile { get; set; }
    public static string[] DupFinderExcludePattern { get; set; }
}

Task("Info")
    .Does(() =>
{
    foreach (var path in GetFiles("*.sln"))
    {
        BuildArtifactSettings.Solution = path;
    }

    Information("Building solution: {0}", BuildArtifactSettings.Solution);
    Information("Configuration: {0}", PipelineSettings.Configuration);
    Information("MSBuild version: {0}", PipelineSettings.ToolVersion);
    Information("Platform: {0}", PipelineSettings.Platform);
});

Task("Clean")
    .IsDependentOn("Info")
    .Does(() =>
{
    MSBuildSettings settings = new MSBuildSettings()
        .SetConfiguration(PipelineSettings.Configuration)
        .SetMSBuildPlatform(PipelineSettings.Platform)
        .UseToolVersion(PipelineSettings.ToolVersion)
        .WithTarget("Clean");

    CleanDirectory(BuildArtifactSettings.ArtifactsDir);
    MSBuild(BuildArtifactSettings.Solution, settings);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(BuildArtifactSettings.Solution);
});

Task("Build")
    .IsDependentOn("Restore") 
    .Does(() =>
{
    MSBuildSettings settings = new MSBuildSettings()
        .SetConfiguration(PipelineSettings.Configuration)
        .SetMSBuildPlatform(PipelineSettings.Platform)
        .UseToolVersion(PipelineSettings.ToolVersion);

    if (PipelineSettings.DoTreatWarningsAsErrors)
    {
        settings.WithProperty("TreatWarningsAsErrors", new string[] { "true" });
    }

    MSBuild(BuildArtifactSettings.Solution, settings);
});

Task("VSTest")
    .IsDependentOn("Build")
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactSettings.OpenCoverDir);
    EnsureDirectoryExists(BuildArtifactSettings.VsTestDir);
    CleanDirectory(BuildArtifactSettings.TestResultsDir);

    OpenCover(
        tool => { tool.VSTest("**/bin/" + PipelineSettings.Configuration + "/" + PipelineSettings.TestDllWhitelist, new VSTestSettings().WithVisualStudioLogger()); },
        BuildArtifactSettings.OpenCoverXml,
        new OpenCoverSettings() { ReturnTargetCodeOffset = 0 }
            .WithFilter(PipelineSettings.OpenCoverFilter)
            .ExcludeByFile(PipelineSettings.OpenCoverExcludeByFile));
})
.Finally(() =>
{
    CopyFiles(BuildArtifactSettings.TestResultsDir + "/*", BuildArtifactSettings.VsTestDir);
    ReportGenerator(BuildArtifactSettings.OpenCoverXml, BuildArtifactSettings.OpenCoverDir);
    ReportUnit(BuildArtifactSettings.TestResultsDir, BuildArtifactSettings.VsTestDir, new ReportUnitSettings());
});

Task("VSMetrics")
    .IsDependentOn("Build")
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactSettings.VsMetricsDir);

    var parsedSolution = ParseSolution(BuildArtifactSettings.Solution);
    var projectOutputs = new FilePathCollection(new PathComparer(false));

    foreach (var project in parsedSolution.Projects)
    {
        var partialPath = project.Name + "/bin/" + PipelineSettings.Configuration + "/" + project.Name;
        projectOutputs.Add(GetFiles(partialPath + ".exe"));
        projectOutputs.Add(GetFiles(partialPath + ".dll"));
    }

    VsMetrics(projectOutputs, BuildArtifactSettings.VsMetricsXml);
});

Task("DupFinder")
    .IsDependentOn("Info")
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactSettings.DupFinderDir);

    DupFinder(BuildArtifactSettings.Solution, new DupFinderSettings {
        ShowStats = true,
        ShowText = true,
        OutputFile = BuildArtifactSettings.DupFinderXml,
        ThrowExceptionOnFindingDuplicates = true,
        ExcludePattern = PipelineSettings.DupFinderExcludePattern });
})
.Finally(() =>
{
    ReSharperReports(BuildArtifactSettings.DupFinderXml, BuildArtifactSettings.DupFinderHtml);
});

Task("InspectCode")
    .IsDependentOn("Info")
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactSettings.InspectCodeDir);

    InspectCode(BuildArtifactSettings.Solution, new InspectCodeSettings {
        SolutionWideAnalysis = true,
        OutputFile = BuildArtifactSettings.InspectCodeXml,
        ThrowExceptionOnFindingViolations = true });
})
.Finally(() =>
{
    ReSharperReports(BuildArtifactSettings.InspectCodeXml, BuildArtifactSettings.InspectCodeHtml);
});
