#addin nuget:?package=Cake.ReSharperReports&version=0.6.0

public static class BuildArtifactParameters
{
    public static DirectoryPath MSTestResultsDir { get; set; }
    public static DirectoryPath AnalysisDir { get; set; }
    public static DirectoryPath OpenCoverDir { get; set; }
    public static FilePath OpenCoverXml { get; set; }
    public static DirectoryPath MSTestDir { get; set; }
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
    public static FilePathCollection MSTestFiles { get; set; }
}

// A solution folder is a simple folder which can be used to organize projects in a solution
private bool IsSolutionFolder(SolutionProject project)
{
    return project.Type.Equals("{2150E333-8FDC-42A3-9474-1A3956D46DE8}");
}

Task("Initialize")
    .Description("Cake.Mug: Initializes all values")
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

    // The TestResults directory is set relative to the current working directory as we cannot specify
    // an alternative tool path for MSTest (even though MSTestSettings offers a WorkingDirectory property)
    BuildArtifactParameters.MSTestResultsDir = new DirectoryPath("TestResults");
    BuildArtifactParameters.AnalysisDir = new DirectoryPath(BuildParameters.ArtifactsDir + "/Analysis");
    BuildArtifactParameters.OpenCoverDir = new DirectoryPath(BuildArtifactParameters.AnalysisDir + "/OpenCover");
    BuildArtifactParameters.OpenCoverXml = new FilePath(BuildArtifactParameters.OpenCoverDir + "/openCover.xml");
    BuildArtifactParameters.MSTestDir = new DirectoryPath(BuildArtifactParameters.AnalysisDir + "/MSTest");
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

    BuildArtifactParameters.MSTestFiles = GetFiles(BuildArtifactParameters.OutputDir + "/**/" + BuildParameters.TestDllWhitelist);
});

Task("Info")
    .Description("Cake.Mug: Prints vital information")
    .IsDependentOn("Initialize")
    .Does(() =>
{
    Information("Solution directory: {0}", BuildParameters.SolutionDir);
    Information("Solution: {0}", BuildParameters.Solution);
    Information("Treating warnings as errors: {0}", BuildParameters.DoTreatWarningsAsErrors);
    Information("Configuration: {0}", BuildParameters.Configuration);
    Information("Artifacts are saved to: {0}", BuildParameters.ArtifactsDir);
});

Task("Clean")
    .Description("Cake.Mug: Cleans the solution, the build artifacts folder and the 'TestResults' folder")
    .IsDependentOn("Info")
    .Does(() =>
{
    MSBuildSettings settings = new MSBuildSettings()
        .SetConfiguration(BuildParameters.Configuration)
        .WithTarget("Clean");

    CleanDirectory(BuildArtifactParameters.MSTestResultsDir);
    CleanDirectory(BuildParameters.ArtifactsDir);

    MSBuild(BuildParameters.Solution, settings);
});

Task("Restore")
    .Description("Cake.Mug: Restores all NuGet packages used by the solution")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(BuildParameters.Solution);
});

Task("Build")
    .Description("Cake.Mug: Builds the solution")
    .IsDependentOn("Restore")
    .Does(() =>
{
    MSBuildSettings settings = new MSBuildSettings()
        .SetConfiguration(BuildParameters.Configuration);

    if (BuildParameters.DoTreatWarningsAsErrors)
    {
        settings.WithProperty("TreatWarningsAsErrors", "true");
    }

    MSBuild(BuildParameters.Solution, settings);

    foreach (var project in ParseSolution(BuildParameters.Solution).Projects)
    {
        if (IsSolutionFolder(project))
        {
            continue;
        }

        var outputPath = project.Path.GetDirectory() + "/bin/**/" + BuildParameters.Configuration + "/**/*";
        var destinationDir = new DirectoryPath(BuildArtifactParameters.OutputDir + "/" + project.Name);

        EnsureDirectoryExists(destinationDir);
        CopyFiles(outputPath, destinationDir, true);
        DeleteFiles(destinationDir + "/*.CodeAnalysisLog.xml");
        DeleteFiles(destinationDir + "/*.lastcodeanalysissucceeded");
    }
});

Task("MSTest")
    .Description("Cake.Mug: Runs MSTest and OpenCover")
    .IsDependentOn("Build")
    .WithCriteria(() => BuildArtifactParameters.MSTestFiles.Count > 0)
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactParameters.OpenCoverDir);
    EnsureDirectoryExists(BuildArtifactParameters.MSTestDir);

    OpenCover(
        tool => { tool.MSTest(BuildArtifactParameters.MSTestFiles); },
        BuildArtifactParameters.OpenCoverXml,
        new OpenCoverSettings() { ReturnTargetCodeOffset = 0 }
            .WithFilter(BuildParameters.OpenCoverFilter)
            .ExcludeByFile(BuildParameters.OpenCoverExcludeByFile));
})
.Finally(() =>
{
    CopyFiles(BuildArtifactParameters.MSTestResultsDir + "/*", BuildArtifactParameters.MSTestDir);
    ReportGenerator(BuildArtifactParameters.OpenCoverXml, BuildArtifactParameters.OpenCoverDir);
    ReportUnit(BuildArtifactParameters.MSTestResultsDir, BuildArtifactParameters.MSTestDir, new ReportUnitSettings());
});

Task("DupFinder")
    .Description("Cake.Mug: Analyses the solution using dupfinder.exe")
    .IsDependentOn("Info")
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactParameters.DupFinderDir);

    DupFinder(BuildParameters.Solution, new DupFinderSettings {
        ShowStats = true,
        ShowText = true,
        OutputFile = BuildArtifactParameters.DupFinderXml,
        ThrowExceptionOnFindingDuplicates = BuildParameters.DupFinderThrowExceptionIfDuplication,
        ExcludePattern = BuildParameters.DupFinderExcludePattern });
})
.Finally(() =>
{
    ReSharperReports(BuildArtifactParameters.DupFinderXml, BuildArtifactParameters.DupFinderHtml);
});

Task("InspectCode")
    .Description("Cake.Mug: Analyses the solution using inspectcode.exe")
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
    .Description("Cake.Mug: A wrapper task for all analytical tasks")
    .IsDependentOn("MSTest")
    .IsDependentOn("DupFinder")
    .IsDependentOn("InspectCode")
    .Does(() =>
{
});

Task("CreateChocolateyPackages")
    .Description("Cake.Mug: Creates Chocolatey packages")
    .IsDependentOn("Build")
    .WithCriteria(() => PackageParameters.ChocolateySpecs.Count > 0)
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactParameters.ChocolateyDir);

    foreach (var nuspec in PackageParameters.ChocolateySpecs)
    {
        ChocolateyPack(nuspec, new ChocolateyPackSettings() { OutputDirectory = BuildArtifactParameters.ChocolateyDir });
    }
});

Task("CreateNuGetPackages")
    .Description("Cake.Mug: Creates NuGet packages")
    .IsDependentOn("Build")
    .WithCriteria(() => PackageParameters.NuGetSpecs.Count > 0)
    .Does(() =>
{
    EnsureDirectoryExists(BuildArtifactParameters.NuGetDir);

    foreach (var nuspec in PackageParameters.NuGetSpecs)
    {
        NuGetPack(nuspec, new NuGetPackSettings() { OutputDirectory = BuildArtifactParameters.NuGetDir });
    }
});

Task("CreatePackages")
    .Description("Cake.Mug: A wrapper for tasks that create packages")
    .IsDependentOn("CreateChocolateyPackages")
    .IsDependentOn("CreateNuGetPackages")
    .Does(() =>
{
});

Task("PushChocolateyPackages")
    .Description("Cake.Mug: Pushes Chocolatey packages")
    .IsDependentOn("CreateChocolateyPackages")
    .WithCriteria(() => DirectoryExists(BuildArtifactParameters.ChocolateyDir))
    .Does(() =>
{
    if (PackageParameters.ChocolateyPushSource == null) { throw new ArgumentNullException("PackageParameters.ChocolateyPushSource", "Please provide an URL"); }

    ChocolateyPush(GetFiles(BuildArtifactParameters.ChocolateyDir + "/**/*.nupkg"), new ChocolateyPushSettings() {
        Source = PackageParameters.ChocolateyPushSource });
});

Task("PushNuGetPackages")
    .Description("Cake.Mug: Pushes NuGet packages")
    .IsDependentOn("CreateNuGetPackages")
    .WithCriteria(() => DirectoryExists(BuildArtifactParameters.NuGetDir))
    .Does(() =>
{
    if (PackageParameters.NuGetPushSource == null) { throw new ArgumentNullException("PackageParameters.NuGetPushSource", "Please provide an URL"); }

    NuGetPush(GetFiles(BuildArtifactParameters.NuGetDir + "/**/*.nupkg"), new NuGetPushSettings() {
        Source = PackageParameters.NuGetPushSource });
});

Task("PushPackages")
    .Description("Cake.Mug: A wrapper for tasks that push packages")
    .IsDependentOn("PushChocolateyPackages")
    .IsDependentOn("PushNuGetPackages")
    .Does(() =>
{
});
