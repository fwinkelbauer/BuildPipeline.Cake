#tool nuget:?package=JetBrains.ReSharper.CommandLineTools&version=2017.1.20170428.83814
#tool nuget:?package=OpenCover&version=4.6.519
#tool nuget:?package=ReportGenerator&version=2.5.7
#tool nuget:?package=ReportUnit&version=1.2.1
#tool nuget:?package=ReSharperReports&version=0.4.0

#addin nuget:?package=Cake.ReSharperReports&version=0.6.0
#addin nuget:?package=Cake.VsMetrics&version=0.2.0
#addin nuget:?package=Cake.FileHelpers&version=1.0.4

public static class BuildArtifactParameters
{
    public static DirectoryPath VSTestResultsDir { get; set; }
    public static DirectoryPath AnalysisDir { get; set; }
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
    public static DirectoryPath PackagesDir { get; set; }
    public static DirectoryPath ChocolateyDir { get; set; }
    public static DirectoryPath NuGetDir { get; set; }
    public static DirectoryPath OutputDir { get; set; }
}

public static class BuildParameters
{
    static BuildParameters()
    {
        SolutionDir = new DirectoryPath(".");
        ArtifactsDir = new DirectoryPath("../BuildArtifacts");
        Solution = null;
        Version = null;
        DoTreatWarningsAsErrors = true;
        Configuration = "Release";
        ToolVersion = MSBuildToolVersion.Default;
        Platform = MSBuildPlatform.Automatic;
        TestDllWhitelist = "*.Tests*.dll";
        OpenCoverFilter = "+[*]* -[*Test*]*";
        OpenCoverExcludeByFile = "*/*Designer.cs;*/*.g.cs;*/*.g.i.cs";
        DupFinderExcludePattern = new string[] {};
        MSBuildProperties = new Dictionary<string, string[]>();
        ChocolateySpecs = "../NuSpec/Chocolatey/";
        NuGetSpecs = "../NuSpec/NuGet/";
    }

    public static DirectoryPath SolutionDir { get; set; }
    public static DirectoryPath ArtifactsDir { get; set; }
    public static FilePath Solution { get; set; }
    public static string Version { get; set; }
    public static bool DoTreatWarningsAsErrors { get; set; }
    public static string Configuration { get; set; }
    public static MSBuildToolVersion ToolVersion { get; set; }
    public static MSBuildPlatform Platform { get; set; }
    public static string TestDllWhitelist { get; set; }
    public static string OpenCoverFilter { get; set; }
    public static string OpenCoverExcludeByFile { get; set; }
    public static string[] DupFinderExcludePattern { get; set; }
    public static IDictionary<string, string[]> MSBuildProperties { get; private set; }
    public static DirectoryPath ChocolateySpecs { get; set; }
    public static DirectoryPath NuGetSpecs { get; set; }

    public static void AddMSBuildProperty(string name, params string[] values)
    {
        MSBuildProperties.Add(name, values);
    }
}

// A solution folder is a simple folder which can be used to organize projects in a solution
private bool IsSolutionFolder(SolutionProject project)
{
    return project.Type.Equals("{2150E333-8FDC-42A3-9474-1A3956D46DE8}");
}

Task("Initialize")
    .Does(() =>
{
    if (BuildParameters.Solution == null)
    {
        BuildParameters.SolutionDir = MakeAbsolute(BuildParameters.SolutionDir);

        Information("Searching for solution in directory {0}", BuildParameters.SolutionDir);

        foreach (var path in GetFiles(BuildParameters.SolutionDir + "/*.sln"))
        {
            Information("Found solution {0}", path);
            BuildParameters.Solution = path;
            break;
        }
    }
    else
    {
        BuildParameters.Solution = MakeAbsolute(BuildParameters.Solution);
        BuildParameters.SolutionDir = BuildParameters.Solution.GetDirectory();

        Information("Using specified solution {0}", BuildParameters.Solution);
    }

    if (BuildParameters.Solution == null)
    {
        throw new Exception("Could not find solution (.sln) file");
    }

    BuildParameters.ArtifactsDir = MakeAbsolute(BuildParameters.ArtifactsDir);
    BuildParameters.ChocolateySpecs = MakeAbsolute(BuildParameters.ChocolateySpecs);
    BuildParameters.NuGetSpecs = MakeAbsolute(BuildParameters.NuGetSpecs);

    // the TestResults directory is set relative to the current working directory as we cannot specify
    // an alternative tool path for VSTest (even though VSTestSettings offers a WorkingDirectory property)
    BuildArtifactParameters.VSTestResultsDir = new DirectoryPath("TestResults");
    BuildArtifactParameters.AnalysisDir = new DirectoryPath(BuildParameters.ArtifactsDir + "/Analysis");
    BuildArtifactParameters.OpenCoverDir = new DirectoryPath(BuildArtifactParameters.AnalysisDir + "/OpenCover");
    BuildArtifactParameters.OpenCoverXml = new FilePath(BuildArtifactParameters.OpenCoverDir + "/openCover.xml");
    BuildArtifactParameters.VsTestDir = new DirectoryPath(BuildArtifactParameters.AnalysisDir + "/VSTest");
    BuildArtifactParameters.VsMetricsDir = new DirectoryPath(BuildArtifactParameters.AnalysisDir + "/Metrics");
    BuildArtifactParameters.VsMetricsXml = new FilePath(BuildArtifactParameters.VsMetricsDir + "/metrics.xml");
    BuildArtifactParameters.DupFinderDir = new DirectoryPath(BuildArtifactParameters.AnalysisDir + "/DupFinder");
    BuildArtifactParameters.DupFinderXml = new FilePath(BuildArtifactParameters.DupFinderDir + "/dupFinder.xml");
    BuildArtifactParameters.DupFinderHtml = new FilePath(BuildArtifactParameters.DupFinderDir + "/dupFinder.html");
    BuildArtifactParameters.InspectCodeDir = new DirectoryPath(BuildArtifactParameters.AnalysisDir + "/InspectCode");
    BuildArtifactParameters.InspectCodeXml = new FilePath(BuildArtifactParameters.InspectCodeDir + "/inspectCode.xml");
    BuildArtifactParameters.InspectCodeHtml = new FilePath(BuildArtifactParameters.InspectCodeDir + "/inspectCode.html");
    BuildArtifactParameters.PackagesDir = new DirectoryPath(BuildParameters.ArtifactsDir + "/Packages");
    BuildArtifactParameters.ChocolateyDir = new DirectoryPath(BuildArtifactParameters.PackagesDir + "/Chocolatey");
    BuildArtifactParameters.NuGetDir = new DirectoryPath(BuildArtifactParameters.PackagesDir + "/NuGet");
    BuildArtifactParameters.OutputDir = new DirectoryPath(BuildParameters.ArtifactsDir + "/Output");
});

Task("Info")
    .IsDependentOn("Initialize")
    .Does(() =>
{
    Information("Solution directory: {0}", BuildParameters.SolutionDir);
    Information("Solution: {0}", BuildParameters.Solution);
    Information("Treating warnings as errors: {0}", BuildParameters.DoTreatWarningsAsErrors);
    Information("Configuration: {0}", BuildParameters.Configuration);
    Information("Chocolatey .nuspec: {0}", BuildParameters.ChocolateySpecs);
    Information("NuGet .nuspec: {0}", BuildParameters.NuGetSpecs);
    Information("Artifacts are saved to: {0}", BuildParameters.ArtifactsDir);
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

    CleanDirectory(BuildArtifactParameters.VSTestResultsDir);
    CleanDirectory(BuildParameters.ArtifactsDir);

    MSBuild(BuildParameters.Solution, settings);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(BuildParameters.Solution);
});

Task("InjectVersion")
    .IsDependentOn("Initialize")
    .WithCriteria(() => !string.IsNullOrWhiteSpace(BuildParameters.Version))
    .Does(() =>
{
    Information("Injecting version: {0}", BuildParameters.Version);
    var versionRegex = @"\d+\.\d+\.\d+\.\d+|\d+\.\d+\.\d+";

    var assemblyInfoFiles = BuildParameters.SolutionDir + "/**/AssemblyInfo.cs";
    var chocoFiles = BuildParameters.ChocolateySpecs + "/**/*.nuspec";
    var nugetFiles = BuildParameters.NuGetSpecs + "/**/*.nuspec";

    ReplaceRegexInFiles(assemblyInfoFiles, versionRegex, BuildParameters.Version);
    ReplaceRegexInFiles(chocoFiles, versionRegex, BuildParameters.Version);
    ReplaceRegexInFiles(nugetFiles, versionRegex, BuildParameters.Version);
});

Task("Build")
    .IsDependentOn("Restore")
    .IsDependentOn("InjectVersion")
    .Does(() =>
{
    Information("MSBuild version: {0}", BuildParameters.ToolVersion);
    Information("Platform: {0}", BuildParameters.Platform);

    MSBuildSettings settings = new MSBuildSettings()
        .SetConfiguration(BuildParameters.Configuration)
        .SetMSBuildPlatform(BuildParameters.Platform)
        .UseToolVersion(BuildParameters.ToolVersion);

    if (BuildParameters.DoTreatWarningsAsErrors)
    {
        settings.WithProperty("TreatWarningsAsErrors", "true");
    }

    foreach (KeyValuePair<string, string[]> entry in BuildParameters.MSBuildProperties)
    {
        settings.WithProperty(entry.Key, entry.Value);
    }

    MSBuild(BuildParameters.Solution, settings);

    foreach (var project in ParseSolution(BuildParameters.Solution).Projects)
    {
        if (IsSolutionFolder(project))
        {
            continue;
        }

        var outputPath = project.Path.GetDirectory() + "/bin/" + BuildParameters.Configuration + "/**/*";
        var destinationDir = new DirectoryPath(BuildArtifactParameters.OutputDir + "/" + project.Name);

        EnsureDirectoryExists(destinationDir);
        CopyFiles(outputPath, destinationDir, true);
    }
});

Task("VSTest")
    .IsDependentOn("Build")
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactParameters.OpenCoverDir);
    EnsureDirectoryExists(BuildArtifactParameters.VsTestDir);

    OpenCover(
        tool => { tool.VSTest(BuildParameters.SolutionDir + "/**/bin/" + BuildParameters.Configuration + "/" + BuildParameters.TestDllWhitelist, new VSTestSettings().WithVisualStudioLogger()); },
        BuildArtifactParameters.OpenCoverXml,
        new OpenCoverSettings() { ReturnTargetCodeOffset = 0 }
            .WithFilter(BuildParameters.OpenCoverFilter)
            .ExcludeByFile(BuildParameters.OpenCoverExcludeByFile));
})
.Finally(() =>
{
    CopyFiles(BuildArtifactParameters.VSTestResultsDir + "/*", BuildArtifactParameters.VsTestDir);
    ReportGenerator(BuildArtifactParameters.OpenCoverXml, BuildArtifactParameters.OpenCoverDir);
    ReportUnit(BuildArtifactParameters.VSTestResultsDir, BuildArtifactParameters.VsTestDir, new ReportUnitSettings());
});

Task("VSMetrics")
    .IsDependentOn("Build")
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactParameters.VsMetricsDir);

    var projectOutputs = new FilePathCollection(new PathComparer(false));

    foreach (var project in ParseSolution(BuildParameters.Solution).Projects)
    {
        if (IsSolutionFolder(project))
        {
            continue;
        }

        if (project.Path.FullPath.ToLower().Contains("wixproj"))
        {
            Warning("Skipping WiX project");
            continue;
        }

        var partialOutputPath = project.Path.GetDirectory() + "/bin/" + BuildParameters.Configuration + "/" + project.Name;

        projectOutputs.Add(GetFiles(partialOutputPath + ".exe"));
        projectOutputs.Add(GetFiles(partialOutputPath + ".dll"));
    }

    VsMetrics(projectOutputs, BuildArtifactParameters.VsMetricsXml);
});

Task("DupFinder")
    .IsDependentOn("Info")
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactParameters.DupFinderDir);

    DupFinder(BuildParameters.Solution, new DupFinderSettings {
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

    InspectCode(BuildParameters.Solution, new InspectCodeSettings {
        SolutionWideAnalysis = true,
        OutputFile = BuildArtifactParameters.InspectCodeXml,
        ThrowExceptionOnFindingViolations = true });
})
.Finally(() =>
{
    ReSharperReports(BuildArtifactParameters.InspectCodeXml, BuildArtifactParameters.InspectCodeHtml);
});

Task("Analyze")
    .IsDependentOn("VSTest")
    .IsDependentOn("VSMetrics")
    .IsDependentOn("DupFinder")
    .IsDependentOn("InspectCode")
    .Does(() =>
{
});

Task("CreatePackages")
    .IsDependentOn("Build")
    .WithCriteria(() => DirectoryExists(BuildParameters.ChocolateySpecs))
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactParameters.ChocolateyDir);
    EnsureDirectoryExists(BuildArtifactParameters.NuGetDir);

    foreach (var nuspec in GetFiles(BuildParameters.ChocolateySpecs + "/**/*.nuspec"))
    {
        ChocolateyPack(nuspec, new ChocolateyPackSettings() { OutputDirectory = BuildArtifactParameters.ChocolateyDir });
    }

    foreach (var nuspec in GetFiles(BuildParameters.NuGetSpecs + "/**/*.nuspec"))
    {
        NuGetPack(nuspec, new NuGetPackSettings() { OutputDirectory = BuildArtifactParameters.NuGetDir });
    }
});
