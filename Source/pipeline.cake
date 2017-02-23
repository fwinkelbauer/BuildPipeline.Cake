#addin nuget:?package=Cake.ReSharperReports&version=0.6.0
#addin nuget:?package=Cake.VsMetrics&version=0.1.0

public static class EnvironmentSettings
{
    static EnvironmentSettings()
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
        Properties = new Dictionary<string, string[]>();
        TestDllWhitelist = "*Test*.dll";
        VsMetricsFiles = new FilePath[] {};
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
    public static FilePath[] VsMetricsFiles { get; set; }
    public static string OpenCoverFilter { get; set; }
    public static string OpenCoverExcludeByFile { get; set; }
    public static string[] DupFinderExcludePattern { get; set; }
}

Task("Info")
    .Does(() =>
{
    foreach (var path in GetFiles("*.sln"))
    {
        EnvironmentSettings.Solution = path;
    }
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

    CleanDirectories(new string[] { EnvironmentSettings.TestResultsDir.FullPath, EnvironmentSettings.ArtifactsDir.FullPath });
    MSBuild(EnvironmentSettings.Solution, settings);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(EnvironmentSettings.Solution);
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

    MSBuild(EnvironmentSettings.Solution, settings);
});

Task("VSTest")
    .IsDependentOn("Build")
    .Does(() =>
{
    EnsureDirectoryExists(EnvironmentSettings.OpenCoverDir);
    EnsureDirectoryExists(EnvironmentSettings.VsTestDir);

    OpenCover(
        tool => { tool.VSTest("**/bin/" + PipelineSettings.Configuration + "/" + PipelineSettings.TestDllWhitelist, new VSTestSettings().WithVisualStudioLogger()); },
        EnvironmentSettings.OpenCoverXml,
        new OpenCoverSettings() { ReturnTargetCodeOffset = 0 }
            .WithFilter(PipelineSettings.OpenCoverFilter)
            .ExcludeByFile(PipelineSettings.OpenCoverExcludeByFile));
})
.Finally(() =>
{
    CopyFiles(EnvironmentSettings.TestResultsDir + "/*", EnvironmentSettings.VsTestDir);
    ReportGenerator(EnvironmentSettings.OpenCoverXml, EnvironmentSettings.OpenCoverDir);
    ReportUnit(EnvironmentSettings.TestResultsDir, EnvironmentSettings.VsTestDir, new ReportUnitSettings());
});

Task("VSMetrics")
    .WithCriteria(() => PipelineSettings.VsMetricsFiles != null && PipelineSettings.VsMetricsFiles.Length > 0)
    .IsDependentOn("Build")
    .Does(() =>
{
    EnsureDirectoryExists(EnvironmentSettings.VsMetricsDir);

    VsMetrics(PipelineSettings.VsMetricsFiles, EnvironmentSettings.VsMetricsXml);
});

Task("DupFinder")
    .IsDependentOn("Info")
    .Does(() =>
{
    EnsureDirectoryExists(EnvironmentSettings.DupFinderDir);

    DupFinder(EnvironmentSettings.Solution, new DupFinderSettings {
        ShowStats = true,
        ShowText = true,
        OutputFile = EnvironmentSettings.DupFinderXml,
        ThrowExceptionOnFindingDuplicates = true,
        ExcludePattern = PipelineSettings.DupFinderExcludePattern });
})
.Finally(() =>
{
    ReSharperReports(EnvironmentSettings.DupFinderXml, EnvironmentSettings.DupFinderHtml);
});

Task("InspectCode")
    .IsDependentOn("Info")
    .Does(() =>
{
    EnsureDirectoryExists(EnvironmentSettings.InspectCodeDir);

    InspectCode(EnvironmentSettings.Solution, new InspectCodeSettings {
        SolutionWideAnalysis = true,
        OutputFile = EnvironmentSettings.InspectCodeXml,
        ThrowExceptionOnFindingViolations = true });
})
.Finally(() =>
{
    ReSharperReports(EnvironmentSettings.InspectCodeXml, EnvironmentSettings.InspectCodeHtml);
});
