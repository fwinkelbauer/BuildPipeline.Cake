#addin nuget:?package=Cake.ReSharperReports&version=0.6.0
#addin nuget:?package=Cake.VsMetrics&version=0.1.0

FilePath solution;

foreach (var path in GetFiles("*.sln"))
{
    solution = path;
}

var testResultsDir = new DirectoryPath("TestResults");
var artifactsDir = new DirectoryPath("../BuildArtifacts");
var openCoverDir = new DirectoryPath(artifactsDir + "/OpenCover");
var openCoverXml = new FilePath(openCoverDir + "/openCover.xml");
var vsTestDir = new DirectoryPath(artifactsDir + "/VSTest");
var vsMetricsDir = new DirectoryPath(artifactsDir + "/Metrics");
var vsMetricsXml = new FilePath(vsMetricsDir + "/metrics.xml");
var dupFinderDir = new DirectoryPath(artifactsDir + "/DupFinder");
var dupFinderXml = new FilePath(dupFinderDir + "/dupFinder.xml");
var dupFinderHtml = new FilePath(dupFinderDir + "/dupFinder.html");
var inspectCodeDir = new DirectoryPath(artifactsDir + "/InspectCode");
var inspectCodeXml = new FilePath(inspectCodeDir + "/inspectCode.xml");
var inspectCodeHtml = new FilePath(inspectCodeDir + "/inspectCode.html");

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

Task("Clean")
    .Does(() =>
{
    MSBuildSettings settings = new MSBuildSettings()
        .SetConfiguration(PipelineSettings.Configuration)
        .SetMSBuildPlatform(PipelineSettings.Platform)
        .UseToolVersion(PipelineSettings.ToolVersion)
        .WithTarget("Clean");

    CleanDirectories(new string[] { testResultsDir.FullPath, artifactsDir.FullPath });
    MSBuild(solution, settings);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solution);
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

    MSBuild(solution, settings);
});

Task("VSTest")
    .IsDependentOn("Build")
    .Does(() =>
{
    EnsureDirectoryExists(openCoverDir);
    EnsureDirectoryExists(vsTestDir);

    OpenCover(
        tool => { tool.VSTest("**/bin/" + PipelineSettings.Configuration + "/" + PipelineSettings.TestDllWhitelist, new VSTestSettings().WithVisualStudioLogger()); },
        openCoverXml,
        new OpenCoverSettings() { ReturnTargetCodeOffset = 0 }
            .WithFilter(PipelineSettings.OpenCoverFilter)
            .ExcludeByFile(PipelineSettings.OpenCoverExcludeByFile));
})
.Finally(() =>
{
    CopyFiles(testResultsDir + "/*", vsTestDir);
    ReportGenerator(openCoverXml, openCoverDir);
    ReportUnit(testResultsDir, vsTestDir, new ReportUnitSettings());
});

Task("VSMetrics")
    .WithCriteria(() => PipelineSettings.VsMetricsFiles != null && PipelineSettings.VsMetricsFiles.Length > 0)
    .IsDependentOn("Build")
    .Does(() =>
{
    EnsureDirectoryExists(vsMetricsDir);

    VsMetrics(PipelineSettings.VsMetricsFiles, vsMetricsXml);
});

Task("DupFinder")
    .Does(() =>
{
    EnsureDirectoryExists(dupFinderDir);

    DupFinder(solution, new DupFinderSettings {
        ShowStats = true,
        ShowText = true,
        OutputFile = dupFinderXml,
        ThrowExceptionOnFindingDuplicates = true,
        ExcludePattern = PipelineSettings.DupFinderExcludePattern });
})
.Finally(() =>
{
    ReSharperReports(dupFinderXml, dupFinderHtml);
});

Task("InspectCode")
    .Does(() =>
{
    EnsureDirectoryExists(inspectCodeDir);

    InspectCode(solution, new InspectCodeSettings {
        SolutionWideAnalysis = true,
        OutputFile = inspectCodeXml,
        ThrowExceptionOnFindingViolations = true });
})
.Finally(() =>
{
    ReSharperReports(inspectCodeXml, inspectCodeHtml);
});
