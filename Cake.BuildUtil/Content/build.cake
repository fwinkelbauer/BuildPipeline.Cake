#tool nuget:?package=JetBrains.ReSharper.CommandLineTools&version=2016.3.20161223.160402
#tool nuget:?package=OpenCover&version=4.6.519
#tool nuget:?package=ReportGenerator&version=2.4.5
#tool nuget:?package=ReportUnit&version=1.2.1
#tool nuget:?package=ReSharperReports&version=0.4.0

#addin nuget:?package=Cake.ReSharperReports&version=0.6.0
#addin nuget:?package=Cake.VsMetrics&version=0.2.0
#addin nuget:?package=Cake.FileHelpers&version=1.0.4

public static class BuildArtifactParameters
{
    static BuildArtifactParameters()
    {
        TestResultsDir = new DirectoryPath("TestResults");
        ArtifactsDir = new DirectoryPath("../BuildArtifacts");
        AnalysisDir = new DirectoryPath(ArtifactsDir + "/Analysis");
        OpenCoverDir = new DirectoryPath(AnalysisDir + "/OpenCover");
        OpenCoverXml = new FilePath(OpenCoverDir + "/openCover.xml");
        VsTestDir = new DirectoryPath(AnalysisDir + "/VSTest");
        VsMetricsDir = new DirectoryPath(AnalysisDir + "/Metrics");
        VsMetricsXml = new FilePath(VsMetricsDir + "/metrics.xml");
        DupFinderDir = new DirectoryPath(AnalysisDir + "/DupFinder");
        DupFinderXml = new FilePath(DupFinderDir + "/dupFinder.xml");
        DupFinderHtml = new FilePath(DupFinderDir + "/dupFinder.html");
        InspectCodeDir = new DirectoryPath(AnalysisDir + "/InspectCode");
        InspectCodeXml = new FilePath(InspectCodeDir + "/inspectCode.xml");
        InspectCodeHtml = new FilePath(InspectCodeDir + "/inspectCode.html");
        PackagesDir = new DirectoryPath(ArtifactsDir + "/Packages");
        ChocolateyDir = new DirectoryPath(PackagesDir + "/Chocolatey");
        NuGetDir = new DirectoryPath(PackagesDir + "/NuGet");
        OutputDir = new DirectoryPath(ArtifactsDir + "/Output");
    }

    public static DirectoryPath TestResultsDir { get; set; }
    public static DirectoryPath ArtifactsDir { get; set; }
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
        SolutionDir = ".";
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

Task("Info")
    .Does(() =>
{
    foreach (var path in GetFiles(BuildParameters.SolutionDir + "/*.sln"))
    {
        BuildParameters.Solution = path;
    }

    Information("Source directory: {0}", MakeAbsolute(BuildParameters.SolutionDir));
    Information("Solution: {0}", BuildParameters.Solution);
    Information("Version: {0}", BuildParameters.Version);
    Information("Configuration: {0}", BuildParameters.Configuration);
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

    CleanDirectory(BuildArtifactParameters.TestResultsDir);
    CleanDirectory(BuildArtifactParameters.ArtifactsDir);

    MSBuild(BuildParameters.Solution, settings);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(BuildParameters.Solution);
});

Task("InjectVersion")
    .IsDependentOn("Info")
    .Does(() =>
{
    if (string.IsNullOrWhiteSpace(BuildParameters.Version))
    {
        throw new Exception("BuildParameters.Version cannot be null!");
    }

    var versionRegex = @"\d+\.\d+\.\d+\.\d+|\d+\.\d+\.\d+";

    var assemblyInfoFiles = BuildParameters.SolutionDir + "/**/AssemblyInfo.cs";
    var chocoFiles = BuildParameters.ChocolateySpecs + "/*.nuspec";
    var nugetFiles = BuildParameters.NuGetSpecs + "/*.nuspec";

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

    // A solution folder is a simple folder which can be used to organize projects in a solution
    var solutionFolderType = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";

    foreach (var project in ParseSolution(BuildParameters.Solution).Projects)
    {
        // We want to skip solution folders as they contain no content on their own
        if (project.Type.Equals(solutionFolderType))
        {
            continue;
        }

        var parsedProject = ParseProject(project.Path);
        var outputPath = project.Path.GetDirectory().Combine(parsedProject.OutputPath).FullPath + "/**/*";
        // The output path of WiX projects might contain some variable names which we have to correct using actual values:
        outputPath = outputPath.Replace("$(Platform)", parsedProject.Platform).Replace("$(Configuration)", parsedProject.Configuration);
        var artifactsDir = new DirectoryPath(BuildArtifactParameters.OutputDir + "/" + project.Name);

        EnsureDirectoryExists(artifactsDir);
        CopyFiles(outputPath, artifactsDir, true);
    }
});

Task("VSTest")
    .IsDependentOn("Build")
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactParameters.OpenCoverDir);
    EnsureDirectoryExists(BuildArtifactParameters.VsTestDir);

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

    var projectOutputs = new FilePathCollection(new PathComparer(false));

    foreach (var project in ParseSolution(BuildParameters.Solution).Projects)
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
    .IsDependentOn("Analyze")
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
