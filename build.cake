//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target                  = Argument("target", "Default");
var configuration           = Argument<string>("configuration", "Release");
var buildNumber             = HasArgument("BuildNumber") ? Argument<int>("BuildNumber") : TFBuild.IsRunningOnVSTS ? int.Parse(TFBuild.Environment.Build.Number) : 0;
var branch                  = TFBuild.IsRunningOnVSTS && EnvironmentVariable("BUILD_SOURCEBRANCH") != null ? EnvironmentVariable("BUILD_SOURCEBRANCH") : "dev"; // TFBuild.Environment.Repository.Branch doesn't provide the full branch name with a forward-slash
var isBranchForRelease      = branch.Contains("rel/"); // release branch convention is rel/{version};
var versionSuffix           = !isBranchForRelease ? "alpha" : XmlPeek("version.props", "/Project/PropertyGroup/VersionSuffix/text()");

//////////////////////////////////////////////////////////////////////
// DEFINE FILES & DIRECTORIES
//////////////////////////////////////////////////////////////////////

var packageDir		    = Directory("./src/Prospa.Packaging");
var artifactsDir        = (DirectoryPath) Directory("./.artifacts");
var packagesDir         = artifactsDir.Combine("packages");
var packagesStableDir   = artifactsDir.Combine("stable");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
   var settings = new DeleteDirectorySettings { Recursive = true, Force = true };

	if (DirectoryExists(artifactsDir))
	{
		DeleteDirectory(artifactsDir, settings);
	}
});

Task("Init")
    .IsDependentOn("Clean")
    .Does(() =>
{
	Context.Information("Artifacts Directory: {0}", artifactsDir);
	Context.Information("Packages Directory: {0}", packagesDir);
    Context.Information("Stable Packages Directory: {0}", packagesStableDir);
    Context.Information("Is Branch for release: {0}", isBranchForRelease);
    Context.Information("Branch: {0}", branch);

    Context.Information("Version suffix: {0}", versionSuffix);
});

Task("Pack")
    .IsDependentOn("Init")
    .Does(() =>
{
    var settings = new DotNetCorePackSettings
    {
		Configuration = configuration,
        OutputDirectory = packagesDir.FullPath
    };

    if (isBranchForRelease)
	{
		if (!string.IsNullOrWhiteSpace(versionSuffix))
		{
			 settings.VersionSuffix = "rel-" + versionSuffix + "-" + buildNumber.ToString("D4");
		}
		else if (string.IsNullOrWhiteSpace(versionSuffix))
		{
			 settings.VersionSuffix = "rel-" + buildNumber.ToString("D4");
		}
	}
	else
	{
		 settings.VersionSuffix = versionSuffix + "-" + buildNumber.ToString("D4");
	}

    Context.Information("Packing pre-release artifacts. Version suffix: " + settings.VersionSuffix);
    DotNetCorePack(packageDir, settings);
});

// When on a release branch (rel/{version}) re-pack without building to drop the build number from the package version).
// This is to allow publishing packages without having version gabs between stable releases (by dropping the build number)
// and avoiding having to rebuild the source.
Task("PackRelease")
    .WithCriteria(() => isBranchForRelease)
    .IsDependentOn("Pack")
    .Does(() =>
{
    var releaseSettings = new DotNetCorePackSettings
    {
        Configuration = configuration,
        OutputDirectory = packagesStableDir.FullPath,
        NoBuild = true
    };

    if (!string.IsNullOrEmpty(versionSuffix))
    {
        releaseSettings.VersionSuffix = versionSuffix;
    }

    Context.Information("Packing release artifacts. Version suffix: " + releaseSettings.VersionSuffix);
    DotNetCorePack(packageDir, releaseSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
  .IsDependentOn("PackRelease");

RunTarget(target);