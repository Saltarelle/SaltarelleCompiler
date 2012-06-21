Framework "4.0x86"

properties {
	$base_dir = Resolve-Path ".."
	$buildtools_dir = Resolve-Path "."
	$out_dir = "$(Resolve-Path "".."")\bin"
	$configuration = "Debug"
	$release_tag_pattern = "release-(.*)"
}

Task default -Depends Build

Task Clean {
	if (Test-Path $out_dir) {
		rm -Recurse -Force "$out_dir" >$null
	}
	md "$out_dir" >$null
}

Task Build-Compiler -Depends Clean, Generate-VersionInfo {
	Exec { msbuild "$base_dir\Saltarelle.Compiler.sln" /verbosity:minimal /p:"Configuration=$configuration" }
}

Task Run-Tests {
@"
	$test_assemblies_file = "$base_dir\Saltarelle\TestAssemblies.txt"

	if (Test-Path "$test_assemblies_file") {
		$testasms = @(Get-Content "$test_assemblies_file")
	}
	else {
		$testasms = @()
	}
	
	if ($testasms.Count -ne 0) {
		$runner = (dir "$base_dir\Saltarelle\packages" -Recurse -Filter nunit-console.exe | Select -ExpandProperty FullName)
		Exec { & "$runner" $testasms -nologo -xml "$out_dir\TestResults.xml" }
	}
"@ >out-null
}

Task Configure -Depends Generate-VersionInfo {
}

Function Determine-PathVersion($RefCommit, $RefVersion, $Path) {
	$revision = ((git log "$RefCommit..HEAD" --pretty=format:"%H" -- (@($Path) | % { """$_""" })) | Measure-Object).Count # Number of commits since our reference commit
	if ($revision -gt 0) {
		New-Object System.Version($RefVersion.Major, $RefVersion.Minor, $RefVersion.Build, $revision)
	}
	else {
		$RefVersion
	}
}

Task Determine-Version {
	$refcommit = % {
	(git log --decorate=full --simplify-by-decoration --pretty=oneline HEAD |           # Append items from the log
		Select-String '\(' |                                                            # Only include entries with names
		% { ($_ -replace "^[^(]*\(([^)]*)\).*$","`$1" -replace " ", "").Split(',') } |  # Select only the names, one line per name, delete spaces
		Select-String "^tag:$release_tag_pattern`$" |                                   # Only tags of interest
		% { $_ -replace "^tag:","" }                                                    # Remove the tag: prefix
	) } { git log --reverse --pretty=format:%H | Select-Object -First 1 } |             # Add the oldest commit as a fallback
	Select-Object -First 1
	
	If ($refcommit | Select-String "^$release_tag_pattern`$") {
		$refVersion = New-Object System.Version(($refcommit -replace "^$release_tag_pattern`$","`$1"))
		If ($refVersion.Build -eq -1) {
			$refVersion = New-Object System.Version($ver.Major, $ver.Minor, 0)
		}
	}
	else {
		$refVersion = New-Object System.Version("0.0.0")
	}
	$script:CompilerVersion = Determine-PathVersion -RefCommit $refCommit -RefVersion $refVersion -Path "$base_dir\Compiler"
	$script:RuntimeVersion = Determine-PathVersion -RefCommit $refCommit -RefVersion $refVersion -Path "$base_dir\Runtime"

	"Compiler version: $script:CompilerVersion"
	"Runtime version: $script:RuntimeVersion"
}

Function Generate-VersionFile($Path, $Version) {
@"
[assembly: System.Reflection.AssemblyVersion("$Version")]
[assembly: System.Reflection.AssemblyFileVersion("$Version")]
"@ | Out-File $Path -Encoding "UTF8"
}

Task Generate-VersionInfo -Depends Determine-Version {
	Generate-VersionFile -Path "$base_dir\Compiler\CompilerVersion.cs" -Version $script:CompilerVersion
	Generate-VersionFile -Path "$base_dir\Runtime\RuntimeVersion.cs" -Version $script:RuntimeVersion
	Generate-VersionFile -Path "$base_dir\Saltarelle\Executables\ExecutablesVersion.cs" -Version $script:ExecutablesVersion
	Generate-VersionFile -Path "$base_dir\Saltarelle\Saltarelle.UI\SaltarelleUIVersion.cs" -Version $script:UIVersion
	Generate-VersionFile -Path "$base_dir\Saltarelle\Saltarelle.Mvc\Properties\SaltarelleMvcVersion.cs" -Version $script:MvcVersion
	Generate-VersionFile -Path "$base_dir\Saltarelle\Saltarelle.CastleWindsor\Properties\SaltarelleCastleWindsorVersion.cs" -Version $script:CastleWindsorVersion

@"
<?xml version="1.0" encoding="utf-8"?>
<Include>
	<?define ProductVersion="$script:ProductVersion"?>
	<?define AssemblyVersion="$script:ExecutablesVersion"?>
</Include>
"@ | Out-File "$base_dir\Saltarelle\VSIntegrationInstaller\Version.wxi" -Encoding UTF8
}
