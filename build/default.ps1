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
	$exedir = "$base_dir\Compiler\SCExe\bin"
	Exec { & "$buildtools_dir\ilmerge.exe" /ndebug "/targetplatform:v4,C:\Windows\Microsoft.NET\Framework\v4.0.30319" "/out:$out_dir\sc.exe" "$exedir\sc.exe" "$exedir\Saltarelle.Compiler.JSModel.dll" "$exedir\Saltarelle.Compiler.dll" "$exedir\ICSharpCode.NRefactory.dll" "$exedir\ICSharpCode.NRefactory.CSharp.dll" "$exedir\Mono.Cecil.dll" }
}

Task Build-Runtime -Depends Clean, Generate-VersionInfo, Build-Compiler {
	Exec { msbuild "$base_dir\ScriptSharp\src\Runtime.sln" /verbosity:minimal /p:"Configuration=$configuration" }
	copy "$base_dir\ScriptSharp\bin\mscorlib.xml" "$out_dir"
	copy "$base_dir\ScriptSharp\bin\mscorlib.dll" "$out_dir"
	copy "$base_dir\ScriptSharp\bin\mscorlib.js" "$out_dir"
	copy "$base_dir\ScriptSharp\bin\mscorlib.debug.js" "$out_dir"
	copy "$base_dir\ScriptSharp\bin\ssloader.js" "$out_dir"
	copy "$base_dir\ScriptSharp\bin\ssloader.debug.js" "$out_dir"
}

Task Run-Tests {
	$runner = (dir "$base_dir\Compiler\packages" -Recurse -Filter nunit-console.exe | Select -ExpandProperty FullName)
	Exec { & "$runner" "$base_dir\Compiler\Compiler.sln" -nologo -xml "$out_dir\TestResults.xml" }
}

Task Build -Depends Build-Compiler, Build-Runtime, Run-Tests {
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
	cd "$base_dir\ScriptSharp"
	$refs = Determine-Ref
	#$script:RuntimeVersion = Determine-PathVersion -RefCommit $refs[0] -RefVersion $refs[1] -Path "$base_dir\ScriptSharp"

	"Compiler version: $script:CompilerVersion"
	#"Runtime version: $script:RuntimeVersion"
	
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
}
