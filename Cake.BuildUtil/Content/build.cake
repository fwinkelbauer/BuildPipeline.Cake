#addin nuget:?package=Cake.ReSharperReports&version=0.6.0
#addin nuget:?package=Cake.VsMetrics&version=0.1.0

public static class BuildArtifactParameters
{
    static BuildArtifactParameters()
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
        ChocolateyDir = new DirectoryPath(ArtifactsDir + "/Chocolatey");
        NuGetDir = new DirectoryPath(ArtifactsDir + "/NuGet");
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
    public static DirectoryPath ChocolateyDir { get; set; }
    public static DirectoryPath NuGetDir { get; set; }
}

public class MSBuildProperty
{
    public MSBuildProperty(string name, params string[] values)
    {
        Name = name;
        Values = values;
    }

    public string Name { get; private set; }
    public string[] Values { get; private set; }
}

public static class BuildParameters
{
    static BuildParameters()
    {
        DoTreatWarningsAsErrors = true;
        Configuration = "Release";
        ToolVersion = MSBuildToolVersion.Default;
        Platform = MSBuildPlatform.Automatic;
        TestDllWhitelist = "*.Tests*.dll";
        OpenCoverFilter = "+[*]* -[*Test*]*";
        OpenCoverExcludeByFile = "*/*Designer.cs;*/*.g.cs;*/*.g.i.cs";
        DupFinderExcludePattern = new string[] {};
        ClickOnceProjects = new FilePath[] {};
        CustomProperties = new Dictionary<string, IList<MSBuildProperty>>();
        ChocolateySpecs = "../NuSpec/Chocolatey/";
        NuGetSpecs = "../NuSpec/NuGet/";
    }

    public static bool DoTreatWarningsAsErrors { get; set; }
    public static string Configuration { get; set; }
    public static MSBuildToolVersion ToolVersion { get; set; }
    public static MSBuildPlatform Platform { get; set; }
    public static string TestDllWhitelist { get; set; }
    public static string OpenCoverFilter { get; set; }
    public static string OpenCoverExcludeByFile { get; set; }
    public static string[] DupFinderExcludePattern { get; set; }
    public static FilePath[] ClickOnceProjects { get; set; }
    public static IDictionary<string, IList<MSBuildProperty>> CustomProperties { get; private set; }
    public static DirectoryPath ChocolateySpecs { get; set; }
    public static DirectoryPath NuGetSpecs { get; set; }

    public static void AddProperty(string config, MSBuildProperty property)
    {
        if (!CustomProperties.ContainsKey(config))
        {
            CustomProperties.Add(config, new List<MSBuildProperty>());
        }

        CustomProperties[config].Add(property);
    }
}

Task("Info")
    .Does(() =>
{
    foreach (var path in GetFiles("*.sln"))
    {
        BuildArtifactParameters.Solution = path;
    }

    Information("Building solution: {0}", BuildArtifactParameters.Solution);
    Information("Configuration: {0}", BuildParameters.Configuration);
    Information("MSBuild version: {0}", BuildParameters.ToolVersion);
    Information("Platform: {0}", BuildParameters.Platform);
});

Task("Clean")
    .IsDependentOn("Info")
    .Does(() =>
{
    MSBuildSettings settings = new MSBuildSettings()
        .SetConfiguration(BuildParameters.Configuration)
        .SetMSBuildPlatform(BuildParameters.Platform)
        .UseToolVersion(BuildParameters.ToolVersion)
        .WithTarget("Clean");

    CleanDirectory(BuildArtifactParameters.ArtifactsDir);
    MSBuild(BuildArtifactParameters.Solution, settings);

    foreach (var wix in GetFiles("**/*.wixproj"))
    {
        MSBuild(wix, settings);
    }
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(BuildArtifactParameters.Solution);
});

Task("Build")
    .IsDependentOn("Restore") 
    .Does(() =>
{
    MSBuildSettings settings = new MSBuildSettings()
        .SetConfiguration(BuildParameters.Configuration)
        .SetMSBuildPlatform(BuildParameters.Platform)
        .UseToolVersion(BuildParameters.ToolVersion);

    if (BuildParameters.DoTreatWarningsAsErrors)
    {
        settings.WithProperty("TreatWarningsAsErrors", "true");
    }

    foreach (var property in BuildParameters.CustomProperties[BuildParameters.Configuration])
    {
        settings.WithProperty(property.Name, property.Values);
    }

    MSBuild(BuildArtifactParameters.Solution, settings);

    foreach (var wix in GetFiles("**/*.wixproj"))
    {
        MSBuild(wix, settings);
    }

    MSBuildSettings clickOnceSettings = settings.WithTarget("Publish");

    foreach (var clickOnce in BuildParameters.ClickOnceProjects)
    {
        MSBuild(clickOnce, clickOnceSettings);
    }
});

Task("VSTest")
    .IsDependentOn("Build")
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactParameters.OpenCoverDir);
    EnsureDirectoryExists(BuildArtifactParameters.VsTestDir);
    CleanDirectory(BuildArtifactParameters.TestResultsDir);

    OpenCover(
        tool => { tool.VSTest("**/bin/" + BuildParameters.Configuration + "/" + BuildParameters.TestDllWhitelist, new VSTestSettings().WithVisualStudioLogger()); },
        BuildArtifactParameters.OpenCoverXml,
        new OpenCoverSettings() { ReturnTargetCodeOffset = 0 }
            .WithFilter(BuildParameters.OpenCoverFilter)
            .ExcludeByFile(BuildParameters.OpenCoverExcludeByFile));
})
.Finally(() =>
{
    CopyFiles(BuildArtifactParameters.TestResultsDir + "/*", BuildArtifactParameters.VsTestDir);
    ReportGenerator(BuildArtifactParameters.OpenCoverXml, BuildArtifactParameters.OpenCoverDir);
    ReportUnit(BuildArtifactParameters.TestResultsDir, BuildArtifactParameters.VsTestDir, new ReportUnitSettings());
});

Task("VSMetrics")
    .IsDependentOn("Build")
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactParameters.VsMetricsDir);

    var parsedSolution = ParseSolution(BuildArtifactParameters.Solution);
    var projectOutputs = new FilePathCollection(new PathComparer(false));

    foreach (var project in parsedSolution.Projects)
    {
        var partialPath = project.Name + "/bin/" + BuildParameters.Configuration + "/" + project.Name;
        projectOutputs.Add(GetFiles(partialPath + ".exe"));
        projectOutputs.Add(GetFiles(partialPath + ".dll"));
    }

    VsMetrics(projectOutputs, BuildArtifactParameters.VsMetricsXml);
});

Task("DupFinder")
    .IsDependentOn("Info")
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactParameters.DupFinderDir);

    DupFinder(BuildArtifactParameters.Solution, new DupFinderSettings {
        ShowStats = true,
        ShowText = true,
        OutputFile = BuildArtifactParameters.DupFinderXml,
        ThrowExceptionOnFindingDuplicates = true,
        ExcludePattern = BuildParameters.DupFinderExcludePattern });
})
.Finally(() =>
{
    ReSharperReports(BuildArtifactParameters.DupFinderXml, BuildArtifactParameters.DupFinderHtml);
});

Task("InspectCode")
    .IsDependentOn("Info")
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactParameters.InspectCodeDir);

    InspectCode(BuildArtifactParameters.Solution, new InspectCodeSettings {
        SolutionWideAnalysis = true,
        OutputFile = BuildArtifactParameters.InspectCodeXml,
        ThrowExceptionOnFindingViolations = true });
})
.Finally(() =>
{
    ReSharperReports(BuildArtifactParameters.InspectCodeXml, BuildArtifactParameters.InspectCodeHtml);
});

Task("Create-Packages")
    .IsDependentOn("VSTest")
    .WithCriteria(() => DirectoryExists(BuildParameters.ChocolateySpecs))
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactParameters.ChocolateyDir);
    EnsureDirectoryExists(BuildArtifactParameters.NuGetDir);

    foreach (var nuspec in GetFiles(BuildParameters.ChocolateySpecs + "/*.nuspec"))
    {
        ChocolateyPack(nuspec, new ChocolateyPackSettings() { OutputDirectory = BuildArtifactParameters.ChocolateyDir });
    }

    foreach (var nuspec in GetFiles(BuildParameters.NuGetSpecs + "/*.nuspec"))
    {
        NuGetPack(nuspec, new NuGetPackSettings() { OutputDirectory = BuildArtifactParameters.NuGetDir });
    }
});
