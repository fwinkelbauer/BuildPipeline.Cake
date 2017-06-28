public static class PackageParameters
{
    static PackageParameters()
    {
        ChocolateySpecs = new FilePathCollection(PathComparer.Default);
        NuGetSpecs = new FilePathCollection(PathComparer.Default);
    }

    /// <summary>
    /// Gets or sets the source used for a Chocolatey push operation
    /// </summary>
    /// <value>An URL</value>
    public static string ChocolateyPushSource { get; set; }

    /// <summary>
    /// Gets a collection of Chocolatey .nuspec files.
    /// This property controls which packages are created.
    /// </summary>
    /// <value>A collection of .nuspec files</value>
    public static FilePathCollection ChocolateySpecs { get; private set; }

    /// <summary>
    /// Gets or sets the source used for a NuGet push operation
    /// </summary>
    /// <value>An URL</value>
    public static string NuGetPushSource { get; set; }

    /// <summary>
    /// Gets a collection of NuGet .nuspec files.
    /// This property controls which packages are created.
    /// </summary>
    /// <value>A collection of .nuspec files</value>
    public static FilePathCollection NuGetSpecs { get; private set; }
}

public static class BuildParameters
{
    static BuildParameters()
    {
        SolutionDir = new DirectoryPath(".");
        ArtifactsDir = new DirectoryPath("../BuildArtifacts");
        Solution = null;
        DoTreatWarningsAsErrors = true;
        Configuration = "Release";
        TestDllWhitelist = "*Tests*.dll";
        OpenCoverFilter = "+[*]* -[*Test*]*";
        OpenCoverExcludeByFile = "*/*Designer.cs;*/*.g.cs;*/*.g.i.cs";
        DupFinderExcludePattern = new string[] {};
        DupFinderThrowExceptionIfDuplication = true;
    }

    /// <summary>
    /// Gets or sets the solution directory. Use either this option or BuildParameters.Solution
    /// </summary>
    /// <value>A directory containing the solution file</value>
    public static DirectoryPath SolutionDir { get; set; }

    /// <summary>
    /// Gets or sets artifacts directory
    /// </summary>
    /// <value>A directory in which all build artifacts which are created through Cake.Mug are put</value>
    public static DirectoryPath ArtifactsDir { get; set; }

    /// <summary>
    /// Gets or sets the solution file. Use either this option or BuildParameters.SolutionDir. 
    /// </summary>
    /// <value>A solution (*.sln) file</value>
    public static FilePath Solution { get; set; }

    /// <summary>
    /// Gets or sets the DoTreatWarningsAsErrors flag
    /// </summary>
    /// <value>If the MSBuild flag for treating warnings as errors should be used</value>
    public static bool DoTreatWarningsAsErrors { get; set; }

    /// <summary>
    /// Gets or sets the run configuration (e.g. Debug, Release, ...)
    /// </summary>
    /// <value>The run configuration (e.g. Debug, Release, ...)</value>
    public static string Configuration { get; set; }

    /// <summary>
    /// Gets or sets the VSTest project whitelist
    /// </summary>
    /// <value>A pattern which specifies which projects should be executed in Cake.Mug's VSTest task</value>
    public static string TestDllWhitelist { get; set; }

    /// <summary>
    /// Gets or sets the OpenCover filter
    /// </summary>
    /// <value>A filter string</value>
    public static string OpenCoverFilter { get; set; }

    /// <summary>
    /// Gets or sets the OpenCover exclusion filter
    /// </summary>
    /// <value>A filter string (a semicolon separated list of file patterns)</value>
    public static string OpenCoverExcludeByFile { get; set; }

    /// <summary>
    /// Gets or sets the DupFinder exclusion filter
    /// </summary>
    /// <value>An array of excluded files</value>
    public static string[] DupFinderExcludePattern { get; set; }

    /// <summary>
    /// Gets or sets the DupFinder flag to indicate if it should throw an Exception when duplicates are found
    /// </summary>
    /// <value>If DupFinder should throw an Exception when duplicates are found</value>
    public static bool DupFinderThrowExceptionIfDuplication { get; set; }
}
