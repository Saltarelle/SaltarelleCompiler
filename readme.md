# Saltarelle C# to JavaScript Compiler #

This compiler can compile C# into JavaScript. By doing this you can leverage all the advantages of C#, such as static type checking, IntelliSense (the kind that works) and lambda expressions when writing code for the browser. **Never, ever, experience an 'object does not support this property or method' again!!**.

Saltarelle is not an entire framework for web application development (such as GWT), rather it consists of a compiler and a small runtime library that only contains the things necessary for the language features to work. It is, however, designed with interoperability in mind so you can use any existing JavaScript framework with it. It comes with support for jQuery, and other libraries can be used either by authoring an import library (which is just a class / set of classes with special attributes on their members) or by using they 'dynamic' feature of C# 4.

The compiler comes as a command line utility, with options similar to those of csc.exe, and as an MSBuild task (and a corresponding .targets file). 

To get started compiling, see the Getting Started guide in the wiki.

## Obtaining binaries ##

Eventually, the project will be included in the NuGet gallery. For the time being, all packages are available at [teamcity.codebetter.com](http://teamcity.codebetter.com/viewLog.html?buildId=lastSuccessful&buildTypeId=bt720&tab=artifacts).

## Current State ##

The compiler currently works quite well. I still need to test it more in practice before I am ready to release it officially, but there are no known issues except for the features that are on the unsupported list.

### Supported features ###

Many, hopefully everything that is not on the unsupported list. For example, it does support the following C# features:

* ref parameters,
* Type inference,
* generics,
* anonymous types,
* lambdas,
* user-defined operators,
* method and constructor overloads,
* Object and collection initializers,
* foreach,
* using,
* Exception handling (although the handling of script exceptions could be improved in the runtime library),
* Named and default arguments,
* C# Variable capture semantics, so if you declare a variable in an inner block and capture it, the captured variable will not be changed when the variable is changed by an outer scope,
* Ensures that expressions are always evaluated left to right, as the C# standard specifies,
* Automatically implemented properties (and events),
* Nullable types (and lifted operators),
* LINQ (including query expression, as well as all the IEnumerable methods supplied by an import library for [Linq.js](http://linqjs.codeplex.com/),
* ... and many more.

### Unsupported features ###

Currently it does not support

* NodeJS (because the runtime library expects to be running in a browser),
* goto (incl. goto case),
* yield break / yield return,
* await (C# 5),
* Multi-dimensional arrays,
* Expression trees,
* operator true / operator false (does anybody use these?),
* "extern alias",
* Clipped integer type (short/byte). It will correctly use integers such that when you assign a double to an integer, or divide two integers, the result will be an integer, but it does not support an integer type which can only have values in the range 0-65535,
* Checked/unchecked,
* User-defined value types (structs)

All these things are on the todo list to address, in the approximate order above, but not until after the official release.

Also, it does not support things that just don't make sense in JavaScript, such as

* pointers
* lock (object) {}

## Credits ##

Saltarelle builds on the tradition of compiling C# to JavaScript pioneered by [Script#](https://github.com/nikhilk/scriptsharp), and also uses a modified version of that project's runtime library. The metadata used to create import libraries is also the same as in Script#.

All analysis of the C# code is done using [NRefactory](https://github.com/icsharpcode/NRefactory), which in turn uses mcs (the compiler from the [mono](http://www.mono-project.com) project and [Mono.Cecil](https://github.com/jbevain/cecil).

## License ##

The entire project is licensed under the [Apache License 2.0](http://www.apache.org/licenses/LICENSE-2.0.html), which is a [permissive](http://www.apache.org/foundation/license-faq.html#WhatDoesItMEAN) license, so there is no issue using the software in any kind of application, commercial or non-commercial. The reason for this license is that it is the one used in the runtime library (which is licensed by Nikhil Kothari, not me).

## Contributing ##

Any contribution is very welcome. You can contribute by reporting an issue, by creating an import library for your favorite JavaScript library, by implementing one of the features on the unsupported list, or by just using the software.

## Building the Compiler ##

To build the compiler and all libraries, open PowerShell, go to the build directory and type `.\psake.ps1`. This will generate all outputs in the bin\ directory, of which the .nupkg files are the real artifacts. After running the build script once, you can build both the compiler and the runtime library directly from Visual Studio, *but you can't build either from VS until you have run the build script once*.
