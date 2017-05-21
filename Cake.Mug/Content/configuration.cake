public static class BuildParameters
{
    static BuildParameters()
    {
        SolutionDir = new DirectoryPath(".");
        ArtifactsDir = new DirectoryPath("../BuildArtifacts");
        Solution = null;
        DoTreatWarningsAsErrors = true;
        Configuration = "Release";
        TestDllWhitelist = "*.Tests*.dll";
        OpenCoverFilter = "+[*]* -[*Test*]*";
        OpenCoverExcludeByFile = "*/*Designer.cs;*/*.g.cs;*/*.g.i.cs";
        DupFinderExcludePattern = new string[] {};
        ChocolateySpecs = "../NuSpec/Chocolatey/";
        NuGetSpecs = "../NuSpec/NuGet/";
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
    /// Gets or sets the specified Chocolatey NuSpec directory
    /// </summary>
    /// <value>A directory containg *.nuspec Chocolatey files</value>
    public static DirectoryPath ChocolateySpecs { get; set; }

    /// <summary>
    /// Gets or sets the specified NuGet NuSpec directory
    /// </summary>
    /// <value>A directory containg *.nuspec NuGet files</value>
    public static DirectoryPath NuGetSpecs { get; set; }
}
