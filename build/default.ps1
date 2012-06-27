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
	Exec { msbuild "$base_dir\Compiler\Compiler.sln" /verbosity:minimal /p:"Configuration=$configuration" }
	$exedir  = "$base_dir\Compiler\SCExe\bin"
	$taskdir = "$base_dir\Compiler\SCTask\bin"
	Exec { & "$buildtools_dir\ilmerge.exe" /ndebug "/targetplatform:v4,C:\Windows\Microsoft.NET\Framework\v4.0.30319" "/out:$out_dir\sc.exe" "$exedir\sc.exe" "$exedir\Saltarelle.Compiler.JSModel.dll" "$exedir\Saltarelle.Compiler.dll" "$exedir\ICSharpCode.NRefactory.dll" "$exedir\ICSharpCode.NRefactory.CSharp.dll" "$exedir\Mono.Cecil.dll" }
	Exec { & "$buildtools_dir\ilmerge.exe" /ndebug "/targetplatform:v4,C:\Windows\Microsoft.NET\Framework\v4.0.30319" "/out:$out_dir\SCTask.dll" "$taskdir\SCTask.dll" "$taskdir\Saltarelle.Compiler.JSModel.dll" "$taskdir\Saltarelle.Compiler.dll" "$taskdir\ICSharpCode.NRefactory.dll" "$taskdir\ICSharpCode.NRefactory.CSharp.dll" "$taskdir\Mono.Cecil.dll" }
	copy "$base_dir\Compiler\SCTask\Saltarelle.Compiler.targets" "$out_dir"
}

Task Build-Runtime -Depends Clean, Generate-VersionInfo, Build-Compiler {
	Exec { msbuild "$base_dir\Runtime\src\Runtime.sln" /verbosity:minimal /p:"Configuration=$configuration" }
}

Task Run-Tests {
	$runner = (dir "$base_dir\Compiler\packages" -Recurse -Filter nunit-console.exe | Select -ExpandProperty FullName)
	Exec { & "$runner" "$base_dir\Compiler\Compiler.sln" -nologo -xml "$out_dir\TestResults.xml" }
}

Task Build-NuGetPackage -Depends Determine-Version {
@"
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>Saltarelle.Compiler</id>
		<version>$script:CompilerVersion</version>
		<title>Saltarelle C# to JavaScript compiler</title>
		<description>Installing this package will transform the project to compile to JavaScript.</description>
		<authors>Erik Källén</authors>
		<projectUrl>https://github.com/erik-kallen/SaltarelleCompiler</projectUrl>
	</metadata>
	<files>
		<file src="$out_dir\dummy.txt" target="content"/>
		<file src="$base_dir\Compiler\install.ps1" target="tools"/>
		<file src="$out_dir\SCTask.dll" target="tools"/>
		<file src="$out_dir\sc.exe" target="tools"/>
		<file src="$base_dir\Compiler\SCTask\Saltarelle.Compiler.targets" target="tools"/>
	</files>
</package>
"@ | Out-File -Encoding UTF8 "$out_dir\SaltarelleCompiler.nuspec"

@"
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>Saltarelle.Runtime</id>
		<version>$script:RuntimeVersion</version>
		<title>Runtime library for the Saltarelle C# to JavaScript compiler</title>
		<description>Runtime library for projects compiled with Saltarelle.Compiler. This is a slightly modified version of the Script# runtime library by Nikhil Kothari (https://github.com/nikhilk/scriptsharp).</description>
		<licenseUrl>http://www.apache.org/licenses/LICENSE-2.0.txt</licenseUrl>
		<authors>Nikhil Kothari, Erik Källén</authors>
		<projectUrl>https://github.com/erik-kallen/SaltarelleCompiler</projectUrl>
		<dependencies>
			<dependency id="Saltarelle.Compiler" version="0.0"/>
		</dependencies>
	</metadata>
	<files>
		<file src="$base_dir\Runtime\bin\mscorlib.dll" target="tools\Assemblies"/>
		<file src="$base_dir\Runtime\bin\mscorlib.xml" target="tools\Assemblies"/>
		<file src="$base_dir\Runtime\bin\mscorlib.js" target="tools\Scripts"/>
		<file src="$base_dir\Runtime\bin\mscorlib.debug.js" target="tools\Scripts"/>
		<file src="$out_dir\dummy.txt" target="content"/>
		<file src="$base_dir\Runtime\src\Libraries\CoreLib\install.ps1" target="tools"/>
	</files>
</package>
"@ | Out-File -Encoding UTF8 "$out_dir\SaltarelleRuntime.nuspec"

@"
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>Saltarelle.Loader</id>
		<version>$script:LoaderVersion</version>
		<title>Package of the Script# script loader for use with the Saltarelle C# to JavaScript compiler</title>
		<description>This is a package of the script loader from the Script# project by Nikhil Kothari (https://github.com/nikhilk/scriptsharp).</description>
		<licenseUrl>http://www.apache.org/licenses/LICENSE-2.0.txt</licenseUrl>
		<authors>Nikhil Kothari</authors>
		<projectUrl>https://github.com/erik-kallen/SaltarelleCompiler</projectUrl>
		<dependencies>
			<dependency id="Saltarelle.Runtime" version="0.0"/>
		</dependencies>
	</metadata>
	<files>
		<file src="$base_dir\Runtime\bin\SSLoader.dll" target="lib"/>
		<file src="$base_dir\Runtime\bin\SSLoader.xml" target="lib"/>
		<file src="$base_dir\Runtime\bin\ssloader.js" target="tools\Scripts"/>
		<file src="$base_dir\Runtime\bin\ssloader.debug.js" target="tools\Scripts"/>
	</files>
</package>
"@ | Out-File -Encoding UTF8 "$out_dir\SaltarelleLoader.nuspec"

	"This file is safe to remove from the project, but NuGet requires the Saltarelle.Compiler package to install something." | Out-File -Encoding UTF8 "$out_dir\dummy.txt"

	Exec { & "$buildtools_dir\nuget.exe" pack "$out_dir\SaltarelleCompiler.nuspec" -OutputDirectory "$out_dir" }
	Exec { & "$buildtools_dir\nuget.exe" pack "$out_dir\SaltarelleRuntime.nuspec" -OutputDirectory "$out_dir" }
	Exec { & "$buildtools_dir\nuget.exe" pack "$out_dir\SaltarelleLoader.nuspec" -OutputDirectory "$out_dir" }
	rm "$out_dir\SaltarelleCompiler.nuspec" > $null
	rm "$out_dir\SaltarelleRuntime.nuspec" > $null
	rm "$out_dir\SaltarelleLoader.nuspec" > $null
	rm "$out_dir\dummy.txt" > $null
}

Task Build -Depends Build-Compiler, Build-Runtime, Run-Tests, Build-NuGetPackage {
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

Function Determine-Ref {
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

	($refcommit, $refVersion)
}

Task Determine-Version {
	$olddir = pwd
	cd "$base_dir\Compiler"
	$refs = Determine-Ref
	$script:CompilerVersion = Determine-PathVersion -RefCommit $refs[0] -RefVersion $refs[1] -Path "$base_dir\Compiler"
	cd "$base_dir\Runtime"
	$refs = Determine-Ref
	$script:RuntimeVersion = Determine-PathVersion -RefCommit $refs[0] -RefVersion $refs[1] -Path "$base_dir\Runtime\src\Libraries\CoreLib","$base_dir\Runtime\src\Core\CoreScript"
	$script:LoaderVersion = Determine-PathVersion -RefCommit $refs[0] -RefVersion $refs[1] -Path "$base_dir\Runtime\src\Libraries\LoaderLib","$base_dir\Runtime\src\Core\LoaderScript"

	"Compiler version: $script:CompilerVersion"
	"Runtime version: $script:RuntimeVersion"
	"Loader version: $script:LoaderVersion"
	
	cd $olddir
}

Function Generate-VersionFile($Path, $Version) {
@"
[assembly: System.Reflection.AssemblyVersion("$Version")]
[assembly: System.Reflection.AssemblyFileVersion("$Version")]
"@ | Out-File $Path -Encoding "UTF8"
}

Task Generate-VersionInfo -Depends Determine-Version {
	Generate-VersionFile -Path "$base_dir\Compiler\CompilerVersion.cs" -Version $script:CompilerVersion
	Generate-VersionFile -Path "$base_dir\Runtime\src\Libraries\CoreLib\Properties\Version.cs" -Version $script:RuntimeVersion
	Generate-VersionFile -Path "$base_dir\Runtime\src\Libraries\LoaderLib\Properties\Version.cs" -Version $script:LoaderVersion
}
