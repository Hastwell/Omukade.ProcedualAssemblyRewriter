# Omukade Procedual Assembly Rewriter
![logo](logo.png)

This is a small helper library (AutoPAR) and companion tool (ManualPAR) to make all members of one or more .NET
assemblies public to facilitate development against libraries containing private members. It can be either invoked manually
or for .NET 6 environments, automatically.

Although this tool is intended primarilly to target .NET Standard 2.0 assemblies (as used by Unity Engine-based software),
it will probably work just as well with any other version of .NET assembly supported by Mono.Cecil.

AutoPAR permits this process to occur without the need to distribute modified 3rd-party assembiles pre-processed by any PAR mechanism. (Developers will still need these preprocessed files on-disk for development and complilation purposes.)

## Requirements
* [.NET 6 Runtime or SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) for your platform
* Supports Windows x64 and Linux x64 + ARM64
* For development, Visual Studio 2022 (any edition) with C#

## Usage - ManualPAR
ManualPAR is a companion tool that batch processes assemblies in a folder using AutoPAR. This tool is mainly intended to prepare preprocessed files that can be compiled against.

Windows: ```omukade-par.exe [--parallel] [--dry-run] (source folder)```

Linux: ```omukade-par [--parallel] [--dry-run] (source folder)```

* `(source folder)` - **required**, the folder containing .NET assemblies to update. (The original files will not be changed.)
* `--parallel` - Performance tweak to update assembiles in parallel instead of sequentially. This process is typically already fast enough you probably don't need it.
* `--dry-run` - Do the work, but don't save anything. You probably don't need this either; it's mainly for testing.

A new folder in the same location as the source folder with the suffix `_PAR` appended will be created, and used for output files. (eg, suppling `C:\stuff` will output files to `C:\stuff_PAR`)

## Usage - AutoPAR
AutoPAR is the main library that provides the functionality of PAR. It can be used in either Auto or Manual mode. Both modes provide identical functionality with regard to assembly processing.

### Auto Mode
***Supported in .NET 6 + Only***

Auto Mode will automatically patch assemblies that cannot be found directly in the app's folder on a as-needed basis when first loaded. Use `AssemblyLoadInterceptor.Initialize` with a folder containing the assemblies that should be processed by AutoPAR (typically a subfolder or your Rainer Assemblies folder).
Notably, it should not be directly in the app folder or anywhere the .NET runtime would look by default, as the original assembly will be loaded instead of the processed one.

This uses `AssemblyLoadContext.Default` and the `AssemblyLoadContext.Default.Resolving` event. If your application uses AssemblyLoadContexts, AutoPAR may fail to intercept assembly loads correctly, or interfere with aspects of your application.

### Manual Mode
Manual Mode does not automatically load assemblies for execution, and is mainly intended to process assemblies for later use (eg, for development purposes), or if AutoPAR cannot be used in your environment due to conflicts or lack of support.
The ManualPAR companion tool provides a basic frontend exposing this functionality.

Initialize a new `ParCore` instance, and process assemblies using `ProcessAssembly` or `ProcessAssemblyAndSaveToFile`. You are responsible for loading assemblies from disk, and the subsequent loading of processed assemblies into the .NET runtime and/or saving them back to disk.

## Developing Apps with AutoPAR
Although AutoPAR processes assemblies automatically at run-time, this does not help at compile-time. ManualPAR may be used to prepare assemblies ahead of time to a shared location that is used at compile-time,
with AutoPAR handling this process at run-time. This can be accomplished by:
* Use of `CopyLocal = false` on all PAR assemblies, so assemblies are not copied to the output folder.
* Create a folder in your output folder, copy all needed _unmodified_ assemblies to it, and direct AutoPAR to this folder -OR- use a configuration setting to point AutoPAR to the location of the _unmodified_ assemblies.

AutoPAR must be initialized as soon as possible before any code that could reference the types loaded by AutoPAR is called.
No types from an AutoPAR processed assembly can be used in any class or method used before or during the initialization of AutoPAR, as the .NET runtime will try to read type metadata (that can't be found yet) before AutoPAR is initialized, as this _will_ give you exceptions about loading types.
Consider eg, a bootstraper `Main()` method that then calls the rest of your application defined in another type and method.

Developers leveraging any PAR mechanism (manual or auto) need to be mindful not to accidentially distribute the modified pre-processed assemblies used for compilation, as this may be interpreted as distributing a derived work.
Every effort should be made to ensure that an end-user can make application work by downloading or using an existing install of an official 1st-party package that contains all needed assemblies, and copying those assemblies to whatever folder was configured in AutoPAR. (This can be in-place if eg, the location of an existing install containing the needed assemblies can be determined/supplied.)

### Common Weird Issues
#### AssemblyLoadException before your Main() is even called
Your `Main()`, the class that contains it (eg, the default `Program` for console apps), or a class with a static constructor refers to a type in an assembly processed by AutoPAR. As AutoPAR has not yet been initialized, refering to such classes will fail.

Refactor the code to minimize these references and initializations until after AutoPAR is initialized, then call your other initalization methods as needed.

### Other Notes on Processed Assemblies
* Processed assemblies retain their original assembly name, version, and other attributes.
* Signatures are not currently updated or stripped, and will be (correctly) rendered invalid.

## License
This software is licensed under the terms of the [GNU AGPL v3.0](https://www.gnu.org/licenses/agpl-3.0.en.html)