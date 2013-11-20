# Saltarelle C# to JavaScript Compiler #

Project web site: http://www.saltarelle-compiler.com

## License ##

The entire project is licensed under the [Apache License 2.0](http://www.apache.org/licenses/LICENSE-2.0.html), which is a [permissive](http://www.apache.org/foundation/license-faq.html#WhatDoesItMEAN) license, so there is no issue using the software in any kind of application, commercial or non-commercial. The reason for this license is that it is the one used in the runtime library (which is licensed by Nikhil Kothari, not me).

## Building the Compiler ##

Before attempting to build the compiler, you need to check out all submodules: Go to the directory to where you cloned the project and run `git submodule update --init --recursive`.

To build the compiler and all libraries, open PowerShell, go to the build directory and type `.\psake.ps1`. This will generate all outputs in the bin\ directory, of which the .nupkg files are the real artifacts. After running the build script once, you can build both the compiler and the runtime library directly from Visual Studio, *but you can't build either from VS until you have run the build script once*.
The following options exist (usage: `.\psake.ps1 -properties @{opt1=value1; opt2=value2 ...}`

* configuration: Debug or Release.
* skipTests: Do not run the tests. Useful to use when developing.
* noAsync: Do not compile tests that depend on the platform compiler being able to use C#5 features. Use to build on .net 4.0
* autoVersion: By default the build script will generate incrementing version numbers depending on tags and `git log`. By setting this to $false you can make it only use tags. This option is probably not that useful to you.

## Obtaining binaries ##

All packages can be installed through NuGet, as well as downloaded from the CI server at [teamcity.codebetter.com](http://teamcity.codebetter.com/project.html?projectId=project234&tab=projectOverview). Trunk builds are the latest releases, Develop builds are the most recent (potentially unstable) ones.

## Contributing ##

Any contribution is very welcome. You can contribute by reporting an issue, by creating an import library for your favorite JavaScript library, by implementing one of the features on the unsupported list, or by just using the software.

This project uses the [git-flow](http://nvie.com/posts/a-successful-git-branching-model/) branching model. This means that all work on future versions should be performed on separate branches forked off of the develop branch. Each commit on the master branch must be tagged with a tag with a name like 'release-version', and will be automatically pushed to NuGet.org.
