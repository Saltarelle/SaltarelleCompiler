Framework "4.0x86"

properties {
	$baseDir = Resolve-Path ".."
	$buildtoolsDir = Resolve-Path "."
	$outDir = "$(Resolve-Path "".."")\bin"
	$configuration = "Debug"
	$releaseTagPattern = "release-(.*)"
	$skipTests = $false
	$noAsync = $false
	$autoVersion = $true
}

Function Get-DotNetVersion($RawVersion) {
	Return New-Object System.Version(($RawVersion -Replace "-.*$","")) # Remove any pre-release information
}

Function Get-DependencyVersion($RawVersion) {
	$netVersion = Get-DotNetVersion -RawVersion $RawVersion
	Return New-Object System.Version($netVersion.Major, $netVersion.Minor)
}

Task default -Depends Build

Task Clean {
	if (Test-Path $outDir) {
		rm -Recurse -Force "$outDir" >$null
	}
	md "$outDir" >$null
}

Task Fix-AntlrLocalization {
	$lang = (Get-Culture).TwoLetterISOLanguageName
	If (-not (Test-Path "$baseDir\Compiler\packages-manual\antlr\Tool\Templates\messages\languages\$lang.stg")) {
		Copy "$baseDir\Compiler\packages-manual\antlr\Tool\Templates\messages\languages\en.stg" "$baseDir\Compiler\packages-manual\antlr\Tool\Templates\messages\languages\$lang.stg"
	}
}

Task Build-Compiler -Depends Clean, Generate-VersionInfo, Fix-AntlrLocalization {
	Exec { msbuild "$baseDir\Compiler\Compiler.sln" /verbosity:minimal /p:"Configuration=$configuration" }
	$exedir  = "$baseDir\Compiler\SCExe\bin"
	Exec { & "$buildtoolsDir\EmbedAssemblies.exe" /o "$outDir\sc.exe" /a "$exedir\*.dll" "$exedir\sc.exe" }
	Exec { & "$buildtoolsDir\EmbedAssemblies.exe" /o "$outDir\SCTask.dll" /a "$baseDir\Compiler\SCTaskWorker\bin\*.dll" "$baseDir\Compiler\SCTask\bin\SCTask.dll" }
	copy "$baseDir\Compiler\SCTask\Saltarelle.Compiler.targets" "$outDir"

	md -Force "$outDir\extensibility" > $null
	copy "$baseDir\Compiler\SCExe\bin\*.*" "$outDir\extensibility" > $null
	del "$outDir\extensibility\sc.*" > $null
}

Task Build-QUnit {
	if ($skipTests) {
		return
	}

	$currentBranch = git rev-parse --abbrev-ref HEAD
	$qunitDir = "$baseDir\Runtime\QUnit"
	cd "$qunitDir"
	git fetch -q origin
	git checkout -fq origin/$currentBranch
	git clean -dxfq

	$extensibilityDir = ls "$qunitDir\packages\Saltarelle.Compiler.ExtensibilityDevelopment.*" | Select-Object -First 1 -ExpandProperty FullName
	copy "$outDir\extensibility\*.*" "$extensibilityDir\lib"

	$compilerDir = ls "$qunitDir\packages\Saltarelle.Compiler.[0-9]*" | Select-Object -First 1 -ExpandProperty FullName
	copy "$outDir\SCTask.dll","$outDir\sc.exe","$outDir\Saltarelle.Compiler.targets","$baseDir\Compiler\install.ps1" "$compilerDir\tools"

	$runtimeDir = ls "$qunitDir\packages\Saltarelle.Runtime.*" | Select-Object -First 1 -ExpandProperty FullName
	copy "$baseDir\Runtime\CoreLib\bin\mscorlib.dll" "$runtimeDir\tools\Assemblies"

	Invoke-Psake "$baseDir\Runtime\QUnit\build\default.ps1"
	
	git reset --hard
}

Task Build-RuntimeCode -Depends Clean, Generate-VersionInfo, Build-Compiler {
	Exec { msbuild "$baseDir\Runtime\Runtime.sln" /verbosity:minimal /p:"Configuration=$($configuration)_NoTests" }

	md -Force "$outDir\extensibility" > $null
	copy "$baseDir\Runtime\CoreLib.Plugin\bin\CoreLib.Plugin.*" "$outDir\extensibility" > $null
}

Task Build-RuntimeTests -Depends Clean, Generate-VersionInfo, Build-Compiler, Build-RuntimeCode, Build-QUnit {
	if (-not $skipTests) {
		$actualConfiguration = $configuration # The _TestsOnly configuration does (for some reason) not work because mscorlib is not build. VS gem.
		if ($noAsync) {
			$actualConfiguration += "_NoAsync"
		}
	
		Exec { msbuild "$baseDir\Runtime\Runtime.sln" /verbosity:minimal /p:"Configuration=$actualConfiguration" }
	}
}

Task Run-Tests -Depends Build-Compiler, Build-RuntimeTests {
	if (-not $skipTests) {
		$runner = (dir "$baseDir\Compiler\packages" -Recurse -Filter nunit-console.exe | Select -ExpandProperty FullName)
		Exec { & "$runner" "$baseDir\Compiler\Saltarelle.Compiler.Tests\Saltarelle.Compiler.Tests.csproj" -nologo -xml "$outDir\CompilerTestResults.xml" }
		Exec { & "$runner" "$baseDir\Runtime\CoreLib.Tests\CoreLib.Tests.csproj" -nologo -xml "$outDir\RuntimeTestResults.xml" }
	}
}

Task Build-NuGetPackages -Depends Determine-Version, Run-Tests {
@"
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>Saltarelle.Compiler</id>
		<version>$script:CompilerVersion</version>
		<title>Saltarelle C# to JavaScript compiler</title>
		<description>Installing this package will transform the project to compile to JavaScript.</description>
		<authors>Erik Källén</authors>
		<projectUrl>http://www.saltarelle-compiler.com</projectUrl>
	</metadata>
	<files>
		<file src="$baseDir\License.txt" target=""/>
		<file src="$baseDir\history.txt" target=""/>
		<file src="$outDir\dummy.txt" target="content"/>
		<file src="$baseDir\Compiler\install.ps1" target="tools"/>
		<file src="$outDir\SCTask.dll" target="tools"/>
		<file src="$outDir\sc.exe" target="tools"/>
		<file src="$baseDir\Compiler\SCTask\Saltarelle.Compiler.targets" target="tools"/>
	</files>
</package>
"@ | Out-File -Encoding UTF8 "$outDir\Compiler.nuspec"

@"
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>Saltarelle.Compiler.ExtensibilityDevelopment</id>
		<version>$script:CompilerVersion</version>
		<title>Extensibility development for the Saltarelle C# to JavaScript compiler</title>
		<description>This package contains file necessary to develop plugins for the Saltarelle C# to Javascript compiler. This package is NOT needed during normal usage.</description>
		<authors>Erik Källén</authors>
		<projectUrl>http://www.saltarelle-compiler.com</projectUrl>
	</metadata>
	<files>
		<file src="$baseDir\License.txt" target=""/>
		<file src="$baseDir\history.txt" target=""/>
		<file src="$outDir\extensibility\*.*" target="lib"/>
	</files>
</package>
"@ | Out-File -Encoding UTF8 "$outDir\ExtensibilityDevelopment.nuspec"

@"
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>Saltarelle.Runtime</id>
		<version>$script:RuntimeVersion</version>
		<title>Runtime library for the Saltarelle C# to JavaScript compiler</title>
		<description>Runtime library for projects compiled with Saltarelle.Compiler. This is a slightly modified version of the Script# runtime library by Nikhil Kothari (https://github.com/nikhilk/scriptsharp).</description>
		<licenseUrl>http://www.apache.org/licenses/LICENSE-2.0.txt</licenseUrl>
		<authors>Nikhil Kothari, Erik Källén</authors>
		<projectUrl>http://www.saltarelle-compiler.com</projectUrl>
		<tags>compiler c# javascript web</tags>
		<dependencies>
			<dependency id="Saltarelle.Compiler" version="$(Get-DependencyVersion $script:CompilerVersion)"/>
		</dependencies>
	</metadata>
	<files>
		<file src="$baseDir\License.txt" target=""/>
		<file src="$baseDir\Runtime\CoreLib\bin\mscorlib.dll" target="tools\Assemblies"/>
		<file src="$baseDir\Runtime\CoreLib\bin\mscorlib.xml" target="tools\Assemblies"/>
		<file src="$baseDir\Runtime\CoreLib.Script\bin\mscorlib.js" target=""/>
		<file src="$baseDir\Runtime\CoreLib.Script\bin\mscorlib.min.js" target=""/>
		<file src="$outDir\dummy.txt" target="content"/>
		<file src="$baseDir\Runtime\CoreLib\install.ps1" target="tools"/>
	</files>
</package>
"@ | Out-File -Encoding UTF8 "$outDir\Runtime.nuspec"

	"This file is safe to remove from the project, but NuGet requires the Saltarelle.Compiler package to install something." | Out-File -Encoding UTF8 "$outDir\dummy.txt"

	Exec { & "$buildtoolsDir\nuget.exe" pack "$outDir\Compiler.nuspec" -NoPackageAnalysis -OutputDirectory "$outDir" }
	Exec { & "$buildtoolsDir\nuget.exe" pack "$outDir\ExtensibilityDevelopment.nuspec" -NoPackageAnalysis -OutputDirectory "$outDir" }
	Exec { & "$buildtoolsDir\nuget.exe" pack "$outDir\Runtime.nuspec" -NoPackageAnalysis -OutputDirectory "$outDir" }
}

Task Build -Depends Build-NuGetPackages {
}

Task Configure -Depends Generate-VersionInfo {
}

Function Determine-PathVersion($RefCommit, $RefVersion, $Path) {
	if ($autoVersion) {
		$RefVersion = New-Object System.Version(($RefVersion -Replace "-.*$",""))
		if ($RefVersion.Build -lt 0) {
			$RefVersion = New-Object System.Version($RefVersion.Major, $RefVersion.Minor, 0)
		}
	
		$revision = ((git log "$RefCommit..HEAD" --pretty=format:"%H" -- (@($Path) | % { """$_""" })) | Measure-Object).Count # Number of commits since our reference commit
		if ($revision -gt 0) {
			Return New-Object System.Version($RefVersion.Major, $RefVersion.Minor, $RefVersion.Build, $revision)
		}
	}

	$RefVersion
}

Function Determine-Ref {
	$refcommit = % {
	(git log --decorate=full --simplify-by-decoration --pretty=oneline HEAD |           # Append items from the log
		Select-String '\(' |                                                            # Only include entries with names
		% { ($_ -replace "^[^(]*\(([^)]*)\).*$","`$1" -replace " ", "").Split(',') } |  # Select only the names, one line per name, delete spaces
		Select-String "^tag:$releaseTagPattern`$" |                                     # Only tags of interest
		% { $_ -replace "^tag:","" }                                                    # Remove the tag: prefix
	) } { git log --reverse --pretty=format:%H | Select-Object -First 1 } |             # Add the oldest commit as a fallback
	Select-Object -First 1
	
	If ($refcommit | Select-String "^$releaseTagPattern`$") {
		$refVersion = $refcommit -replace "^$releaseTagPattern`$","`$1"
	}
	else {
		$refVersion = "0.0.0"
	}

	($refcommit, $refVersion)
}

Task Determine-Version {
	if (-not $autoVersion) {
		if ((git log -1 --decorate=full --simplify-by-decoration --pretty=oneline HEAD |
			 Select-String '\(' |
			 % { ($_ -replace "^[^(]*\(([^)]*)\).*$","`$1" -replace " ", "").Split(',') } |
			 Select-String "^tag:$releaseTagPattern`$" |
			 % { $_ -replace "^tag:","" } |
			 Measure-Object
			).Count -eq 0) {
			
			Throw "The most recent commit must be tagged when not using auto-versioning"
		}
	}

	$refs = Determine-Ref
	$script:CompilerVersion = Determine-PathVersion -RefCommit $refs[0] -RefVersion $refs[1] -Path "$baseDir\Compiler"
	$script:RuntimeVersion = Determine-PathVersion -RefCommit $refs[0] -RefVersion $refs[1] -Path "$baseDir\Runtime"

	"Compiler version: $script:CompilerVersion"
	"Runtime version: $script:RuntimeVersion"
}

Function Generate-VersionFile($Path, $Version) {
	$Version = Get-DotNetVersion -RawVersion $Version
@"
[assembly: System.Reflection.AssemblyVersion("$($Version.Major).$($Version.Minor).0.0")]
[assembly: System.Reflection.AssemblyFileVersion("$Version")]
"@ | Out-File $Path -Encoding "UTF8"
}

Task Generate-VersionInfo -Depends Determine-Version {
	Generate-VersionFile -Path "$baseDir\Compiler\CompilerVersion.cs" -Version $script:CompilerVersion
	Generate-VersionFile -Path "$baseDir\Runtime\CoreLib\Properties\Version.cs" -Version $script:RuntimeVersion
}
