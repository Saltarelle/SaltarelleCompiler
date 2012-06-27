Param($installPath, $toolsPath, $package, $project)

Function MakeRelativePath($Origin, $Target) {
    $originUri = New-Object Uri('file://' + $Origin)
    $targetUri = New-Object Uri('file://' + $Target)
    $originUri.MakeRelativeUri($targetUri).ToString().Replace('/', [System.IO.Path]::DirectorySeparatorChar)
}

Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
$msbuild = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1

# Set the NoStdLib property. Not that we need it, but other tools (eg. ReSharper) will have use for it.
$msbuild.SetProperty("NoStdLib", "True")

# Remove default assemblies System, System.*, Microsoft.*
$project.Object.References | ? { $_.Name.StartsWith("System.") } | % { try { $_.Remove() } catch {} }
$project.Object.References | ? { $_.Name -eq "System" } | % { $_.Remove() }
$project.Object.References | ? { $_.Name.StartsWith("Microsoft.") } | % { $_.Remove() }

## Swap the import for Microsoft.CSharp.targets for Saltarelle.Compiler.targets. Also remove any existing reference to Saltarelle.Compiler.targets since we might be upgrading.
$msbuild.Xml.Imports | ? { $_.Project.EndsWith("Saltarelle.Compiler.targets") } | % { $msbuild.Xml.RemoveChild($_) }
$msbuild.Xml.Imports | ? { $_.Project.EndsWith("Microsoft.CSharp.targets") } | % { $msbuild.Xml.RemoveChild($_) }
$msbuild.Xml.AddImport("`$(SolutionDir)$(MakeRelativePath -Origin $project.DTE.Solution.FullName -Target ([System.IO.Path]::Combine($toolsPath, ""Saltarelle.Compiler.targets"")))")

# Remove the dummy file we have to create in order to have our installer being called by NuGet
$project.ProjectItems | ? { $_.Name -eq "dummy.txt" } | % { $_.Delete() }