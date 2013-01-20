Param($installPath, $toolsPath, $package, $project)

Function MakeRelativePath($Origin, $Target) {
    $originUri = New-Object Uri('file://' + $Origin)
    $targetUri = New-Object Uri('file://' + $Target)
    $originUri.MakeRelativeUri($targetUri).ToString().Replace('/', [System.IO.Path]::DirectorySeparatorChar)
}

Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
$msbuild = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1

# Add a reference to our custom mscorlib.dll (adding this reference by putting the file in the lib/ folder does not work).
$old = @($msbuild.GetItems("Reference") | ? { $_.UnevaluatedInclude -eq "mscorlib" })
$old | % { $msbuild.RemoveItem($_) }

$mscorlib = $msbuild.AddItem("Reference", "mscorlib") | Select-Object -First 1
$mscorlib.SetMetadataValue("HintPath", "`$(SolutionDir)$(MakeRelativePath -Origin $project.DTE.Solution.FullName -Target ([System.IO.Path]::Combine($toolsPath, ""Assemblies"", ""mscorlib.dll"")))")

# Remove the dummy file we have to create in order to have our installer being called by NuGet
$project.ProjectItems | ? { $_.Name -eq "dummy.txt" } | % { $_.Delete() }