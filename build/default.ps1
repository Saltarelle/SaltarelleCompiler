Framework "4.0x86"

properties {
	$baseDir = Resolve-Path ".."
	$buildtoolsDir = Resolve-Path "."
	$outDir = "$(Resolve-Path "".."")\bin"
	$configuration = "Debug"
	$releaseTagPattern = "release-(.*)"
}

Task default -Depends Build-AutoVersion

Task Build-AutoVersion {
	$script:autoVersion = $true
	Invoke-Task Build
}

Task Build-NoAutoVersion {
	$script:autoVersion = $false
	Invoke-Task Build
}

Task Clean {
	if (Test-Path $outDir) {
		rm -Recurse -Force "$outDir" >$null
	}
	md "$outDir" >$null
}

Task Build-Compiler -Depends Clean, Generate-VersionInfo {
	Exec { msbuild "$baseDir\Compiler\Compiler.sln" /verbosity:minimal /p:"Configuration=$configuration" }
	$exedir  = "$baseDir\Compiler\SCExe\bin"
	$taskdir = "$baseDir\Compiler\SCTask\bin"
	Exec { & "$buildtoolsDir\ilmerge.exe" /ndebug "/targetplatform:v4,C:\Windows\Microsoft.NET\Framework\v4.0.30319" "/out:$outDir\sc.exe" "/keyfile:$baseDir\Saltarelle.snk" "$exedir\sc.exe" "$exedir\Saltarelle.Compiler.JSModel.dll" "$exedir\Saltarelle.Compiler.dll" "$exedir\ICSharpCode.NRefactory.dll" "$exedir\ICSharpCode.NRefactory.CSharp.dll" "$exedir\Mono.Cecil.dll" }
	Exec { & "$buildtoolsDir\ilmerge.exe" /ndebug "/targetplatform:v4,C:\Windows\Microsoft.NET\Framework\v4.0.30319" "/out:$outDir\SCTask.dll" "/keyfile:$baseDir\Saltarelle.snk" "$taskdir\SCTask.dll" "$taskdir\Saltarelle.Compiler.JSModel.dll" "$taskdir\Saltarelle.Compiler.dll" "$taskdir\ICSharpCode.NRefactory.dll" "$taskdir\ICSharpCode.NRefactory.CSharp.dll" "$taskdir\Mono.Cecil.dll" }
	copy "$baseDir\Compiler\SCTask\Saltarelle.Compiler.targets" "$outDir"
}

Task Generate-jQueryUISource -Depends Determine-Version {
	Exec { msbuild "$baseDir\Runtime\tools\jQueryUIGenerator\jQueryUIGenerator.sln" /verbosity:minimal /p:"Configuration=$configuration" }
	rmdir -Force -Recurse -ErrorAction SilentlyContinue "$baseDir\Runtime\src\Libraries\jQuery\jQuery.UI" | Out-Null
	Exec { & "$baseDir\Runtime\tools\jQueryUIGenerator\jQueryUIGenerator\bin\ScriptSharp.Tools.jQueryUIGenerator.exe" "$baseDir\Runtime\tools\jQueryUIGenerator\jQueryUIGenerator\entries" "$baseDir\Runtime\src\Libraries\jQuery\jQuery.UI" /p | Out-Null }
	Generate-VersionFile -Path "$baseDir\Runtime\src\Libraries\jQuery\jQuery.UI\Properties\Version.cs" -Version $script:JQueryUIVersion
}

Task Build-Runtime -Depends Clean, Generate-VersionInfo, Build-Compiler, Generate-jQueryUISource {
	Exec { msbuild "$baseDir\Runtime\src\Runtime.sln" /verbosity:minimal /p:"Configuration=$configuration" }
}

Task Run-Tests -Depends Build-Compiler, Build-Runtime {
	$runner = (dir "$baseDir\Compiler\packages" -Recurse -Filter nunit-console.exe | Select -ExpandProperty FullName)
	Exec { & "$runner" "$baseDir\Compiler\Compiler.sln" -nologo -xml "$outDir\CompilerTestResults.xml" }
	Exec { & "$runner" "$baseDir\Runtime\src\Tests\RuntimeLibrary.Tests\RuntimeLibrary.Tests.csproj" -nologo -xml "$outDir\RuntimeTestResults.xml" }
}

Task Build-NuGetPackages -Depends Determine-Version {
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
		<file src="$baseDir\Compiler\License.txt" target=""/>
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
		<id>Saltarelle.Runtime</id>
		<version>$script:RuntimeVersion</version>
		<title>Runtime library for the Saltarelle C# to JavaScript compiler</title>
		<description>Runtime library for projects compiled with Saltarelle.Compiler. This is a slightly modified version of the Script# runtime library by Nikhil Kothari (https://github.com/nikhilk/scriptsharp).</description>
		<licenseUrl>http://www.apache.org/licenses/LICENSE-2.0.txt</licenseUrl>
		<authors>Nikhil Kothari, Erik Källén</authors>
		<projectUrl>http://www.saltarelle-compiler.com</projectUrl>
		<dependencies>
			<dependency id="Saltarelle.Compiler" version="0.0"/>
		</dependencies>
	</metadata>
	<files>
		<file src="$baseDir\Runtime\License.txt" target=""/>
		<file src="$baseDir\Runtime\bin\mscorlib.dll" target="tools\Assemblies"/>
		<file src="$baseDir\Runtime\bin\mscorlib.xml" target="tools\Assemblies"/>
		<file src="$baseDir\Runtime\bin\Script\mscorlib.js" target=""/>
		<file src="$baseDir\Runtime\bin\Script\mscorlib.debug.js" target=""/>
		<file src="$baseDir\Runtime\src\Libraries\CoreLib\qunit-1.9.0.js" target=""/>
		<file src="$baseDir\Runtime\src\Libraries\CoreLib\qunit-1.9.0.css" target=""/>
		<file src="$baseDir\history.txt" target=""/>
		<file src="$outDir\dummy.txt" target="content"/>
		<file src="$baseDir\Runtime\src\Libraries\CoreLib\install.ps1" target="tools"/>
	</files>
</package>
"@ | Out-File -Encoding UTF8 "$outDir\Runtime.nuspec"

@"
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>Saltarelle.Linq</id>
		<version>$script:LinqVersion</version>
		<title>Linq for the Saltarelle C# to JavaScript compiler</title>
		<description>Import library for Linq.js (linqjs.codeplex.com) for projects compiled with Saltarelle.Compiler. Unfortunately the official version is slightly incompatible with the Saltarelle runtime so you have to use the JS files included in this package instead of the official linq.js release.</description>
		<licenseUrl>http://www.apache.org/licenses/LICENSE-2.0.txt</licenseUrl>
		<authors>neue.cc, Erik Källén</authors>
		<projectUrl>http://www.saltarelle-compiler.com</projectUrl>
		<dependencies>
			<dependency id="Saltarelle.Runtime" version="0.0"/>
		</dependencies>
	</metadata>
	<files>
		<file src="$baseDir\Runtime\License.txt" target=""/>
		<file src="$baseDir\Runtime\bin\Script.Linq.dll" target="lib"/>
		<file src="$baseDir\Runtime\bin\Script.Linq.xml" target="lib"/>
		<file src="$baseDir\Runtime\bin\Script\linq.js" target=""/>
		<file src="$baseDir\Runtime\bin\Script\linq.min.js" target=""/>
	</files>
</package>
"@ | Out-File -Encoding UTF8 "$outDir\Linq.nuspec"

@"
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>Saltarelle.Loader</id>
		<version>$script:LoaderVersion</version>
		<title>Package of the Script# script loader for use with the Saltarelle C# to JavaScript compiler</title>
		<description>This is a package of the script loader from the Script# project by Nikhil Kothari (https://github.com/nikhilk/scriptsharp).</description>
		<licenseUrl>http://www.apache.org/licenses/LICENSE-2.0.txt</licenseUrl>
		<authors>Nikhil Kothari</authors>
		<projectUrl>http://www.saltarelle-compiler.com</projectUrl>
		<dependencies>
			<dependency id="Saltarelle.Runtime" version="0.0"/>
		</dependencies>
	</metadata>
	<files>
		<file src="$baseDir\Runtime\License.txt" target=""/>
		<file src="$baseDir\Runtime\bin\SSLoader.dll" target="lib"/>
		<file src="$baseDir\Runtime\bin\SSLoader.xml" target="lib"/>
		<file src="$baseDir\Runtime\bin\Script\ssloader.js" target=""/>
		<file src="$baseDir\Runtime\bin\Script\ssloader.debug.js" target=""/>
	</files>
</package>
"@ | Out-File -Encoding UTF8 "$outDir\Loader.nuspec"

@"
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>Saltarelle.Web</id>
		<version>$script:WebVersion</version>
		<title>Metadata required to create HTML5 applications using the Saltarelle C# to JavaScript compiler</title>
		<description>This package contains the required metadata to develop web applications with the Saltarelle C# to JavaScript compiler. It is a slightly modified version of the web library from the Script# project by Nikhil Kothari (https://github.com/nikhilk/scriptsharp).</description>
		<licenseUrl>http://www.apache.org/licenses/LICENSE-2.0.txt</licenseUrl>
		<authors>Nikhil Kothari</authors>
		<projectUrl>http://www.saltarelle-compiler.com</projectUrl>
		<dependencies>
			<dependency id="Saltarelle.Runtime" version="0.0"/>
		</dependencies>
	</metadata>
	<files>
		<file src="$baseDir\Runtime\License.txt" target=""/>
		<file src="$baseDir\Runtime\bin\Script.Web.dll" target="lib"/>
		<file src="$baseDir\Runtime\bin\Script.Web.xml" target="lib"/>
	</files>
</package>
"@ | Out-File -Encoding UTF8 "$outDir\Web.nuspec"

@"
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>Saltarelle.jQuery</id>
		<version>$script:JQueryVersion</version>
		<title>Metadata required to use jQuery with the Saltarelle C# to JavaScript compiler</title>
		<description>This package contains the required metadata to use jQuery with the Saltarelle C# to JavaScript compiler. It is a slightly modified version of the jQuery import library from the Script# project by Nikhil Kothari (https://github.com/nikhilk/scriptsharp).</description>
		<licenseUrl>http://www.apache.org/licenses/LICENSE-2.0.txt</licenseUrl>
		<authors>Nikhil Kothari</authors>
		<projectUrl>http://www.saltarelle-compiler.com</projectUrl>
		<dependencies>
			<dependency id="Saltarelle.Runtime" version="0.0"/>
			<dependency id="Saltarelle.Web" version="0.0"/>
		</dependencies>
	</metadata>
	<files>
		<file src="$baseDir\Runtime\License.txt" target=""/>
		<file src="$baseDir\Runtime\bin\Script.jQuery.dll" target="lib"/>
		<file src="$baseDir\Runtime\bin\Script.jQuery.xml" target="lib"/>
		<file src="$baseDir\Runtime\src\Libraries\jQuery\jQuery.Core\*.js" target=""/>
	</files>
</package>
"@ | Out-File -Encoding UTF8 "$outDir\jQuery.nuspec"

@"
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>Saltarelle.jQuery.UI</id>
		<version>$script:JQueryUIVersion</version>
		<title>Metadata required to use jQuery UI with the Saltarelle C# to JavaScript compiler</title>
		<description>This package contains the required metadata to use jQuery UI with the Saltarelle C# to JavaScript compiler. It is a slightly modified version of the jQuery import library from the Script# project by Nikhil Kothari (https://github.com/nikhilk/scriptsharp).</description>
		<licenseUrl>http://www.apache.org/licenses/LICENSE-2.0.txt</licenseUrl>
		<authors>Ivaylo Gochkov, Erik Källén</authors>
		<projectUrl>http://www.saltarelle-compiler.com</projectUrl>
		<dependencies>
			<dependency id="Saltarelle.Runtime" version="0.0"/>
			<dependency id="Saltarelle.Web" version="0.0"/>
			<dependency id="Saltarelle.jQuery" version="0.0"/>
		</dependencies>
	</metadata>
	<files>
		<file src="$baseDir\Runtime\License.txt" target=""/>
		<file src="$baseDir\Runtime\bin\Script.jQuery.UI.dll" target="lib"/>
		<file src="$baseDir\Runtime\bin\Script.jQuery.UI.xml" target="lib"/>
		<file src="$baseDir\Runtime\src\Libraries\jQuery\jQuery.UI\*.js" target=""/>
	</files>
</package>
"@ | Out-File -Encoding UTF8 "$outDir\jQuery.UI.nuspec"

	"This file is safe to remove from the project, but NuGet requires the Saltarelle.Compiler package to install something." | Out-File -Encoding UTF8 "$outDir\dummy.txt"

	Exec { & "$buildtoolsDir\nuget.exe" pack "$outDir\Compiler.nuspec" -OutputDirectory "$outDir" }
	Exec { & "$buildtoolsDir\nuget.exe" pack "$outDir\Runtime.nuspec" -OutputDirectory "$outDir" }
	Exec { & "$buildtoolsDir\nuget.exe" pack "$outDir\Linq.nuspec" -OutputDirectory "$outDir" }
	Exec { & "$buildtoolsDir\nuget.exe" pack "$outDir\Loader.nuspec" -OutputDirectory "$outDir" }
	Exec { & "$buildtoolsDir\nuget.exe" pack "$outDir\Web.nuspec" -OutputDirectory "$outDir" }
	Exec { & "$buildtoolsDir\nuget.exe" pack "$outDir\jQuery.nuspec" -OutputDirectory "$outDir" }
	Exec { & "$buildtoolsDir\nuget.exe" pack "$outDir\jQuery.UI.nuspec" -OutputDirectory "$outDir" }
}

Task Build -Depends Build-Compiler, Build-Runtime, Run-Tests, Build-NuGetPackages {
}

Task Configure -Depends Generate-VersionInfo {
}

Function Determine-PathVersion($RefCommit, $RefVersion, $Path) {
	if ($script:autoVersion) {
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
	if (-not $script:autoVersion) {
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

	$olddir = pwd
	cd "$baseDir\Compiler"
	$refs = Determine-Ref
	$script:CompilerVersion = Determine-PathVersion -RefCommit $refs[0] -RefVersion $refs[1] -Path "$baseDir\Compiler"

	cd "$baseDir\Runtime"
	$runtimeRefCommit = git log --reverse --pretty=format:%H | Select-Object -First 1
	$script:RuntimeVersion = Determine-PathVersion -RefCommit "$runtimeRefCommit" -RefVersion $refs[1] -Path "src\Libraries\CoreLib","src\Core\CoreScript"
	$script:LinqVersion = Determine-PathVersion -RefCommit "$runtimeRefCommit" -RefVersion $refs[1] -Path "src\Libraries\LinqJS","src\Core\LinqJSScript"
	$script:LoaderVersion = Determine-PathVersion -RefCommit "$runtimeRefCommit" -RefVersion $refs[1] -Path "src\Libraries\LoaderLib","src\Core\LoaderScript"
	$script:WebVersion = Determine-PathVersion -RefCommit "$runtimeRefCommit" -RefVersion $refs[1] -Path "src\Libraries\Web"
	$script:JQueryVersion = Determine-PathVersion -RefCommit "$runtimeRefCommit" -RefVersion $refs[1] -Path "src\Libraries\jQuery\jQuery.Core"
	$script:JQueryUIVersion = Determine-PathVersion -RefCommit "$runtimeRefCommit" -RefVersion $refs[1] -Path "tools\jQueryUIGenerator"

	"Compiler version: $script:CompilerVersion"
	"Runtime version: $script:RuntimeVersion"
	"Linq version: $script:LinqVersion"
	"Loader version: $script:LoaderVersion"
	"Web version: $script:WebVersion"
	"jQuery version: $script:jQueryVersion"
	"jQuery UI version: $script:jQueryUIVersion"

	cd $olddir
}

Function Generate-VersionFile($Path, $Version) {
	$Version = $Version -Replace "-.*$","" # Remove any pre-release information

@"
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("$Version")]
"@ | Out-File $Path -Encoding "UTF8"
}

Task Generate-VersionInfo -Depends Determine-Version {
	Generate-VersionFile -Path "$baseDir\Compiler\CompilerVersion.cs" -Version $script:CompilerVersion
	Generate-VersionFile -Path "$baseDir\Runtime\src\Libraries\CoreLib\Properties\Version.cs" -Version $script:RuntimeVersion
	Generate-VersionFile -Path "$baseDir\Runtime\src\Libraries\LoaderLib\Properties\Version.cs" -Version $script:LoaderVersion
	Generate-VersionFile -Path "$baseDir\Runtime\src\Libraries\LinqJS\Properties\Version.cs" -Version $script:LinqVersion
	Generate-VersionFile -Path "$baseDir\Runtime\src\Libraries\Web\Properties\Version.cs" -Version $script:WebVersion
	Generate-VersionFile -Path "$baseDir\Runtime\src\Libraries\jQuery\jQuery.Core\Properties\Version.cs" -Version $script:JQueryVersion
}
