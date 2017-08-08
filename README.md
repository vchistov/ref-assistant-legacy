# Reference Assistant (student project)
## WARNING
>This project was developed at 2011 and originally hosted by [codeplex](https://refassistant.codeplex.com). I'm not working on the project anymore.

## Project Description
Reference Assistant is free, open source tool to remove unused references from C#, F#, VB.NET or VC++/CLI projects in the Visual Studio 2010/11. It's developed in C#.

## Quick Information
Often a .NET project has some references that are not used by any types of its project.When creates new project, the Visual Studio adds several assemblies by default. Types from these assemblies can be used in the future. How to detect what assembly reference is useful and what is not? Simplest way checking the used types. But what if a project is so big? Well it is possible to check an assembly manifest. Yes, assemblies references which are required for runtime will be included in the manifest. But there is one more problem. The assembly can be required indirectly. For example, if you use some type then all assemblies containing definitions of its base types (classes and interfaces from type's hierarchy) should be in a project's references. It is just one example. The Reference Assistant checks several criterions to decide to remove assembly reference or not:
* checks a project assembly's manifest (for VC++/CLI doesn't check);
* checks the hierarchy of used classes;
* checks the hierarchy of used interfaces;
* checks the used attributes and their parameters types;
* checks the [forwarded types](https://msdn.microsoft.com/en-us/library/system.runtime.compilerservices.typeforwardedtoattribute.aspx)(some type can be moved from one assembly to another);
* checks the imported types (e.g. COM types);
* checks the used members of types (methods, properties, fields, events) and them parameters types;
* checks the types declared in XAML (Silverlight, WPF, Workflow 4.0, Windows Phone)

The Reference Assistant uses the [Mono.Cecil](http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cecil/) library for reading assembly metadata.

## Visual Studio Gallery
The Reference Assistant is available for downloading on the Visual Studio Gallery:
* [Reference Assistant for Visual Studio 2010](http://visualstudiogallery.msdn.microsoft.com/fc504cc6-5808-4da8-ae86-8d3f9ed81606)
* [Reference Assistant for Visual Studio 11](http://visualstudiogallery.msdn.microsoft.com/ff717422-f6a7-4f18-b972-f7540eaf371e)

## Prerequires for Developers
If you want to change the Reference Assitant's source code for yourself and to compile it after that, you need to install Visual Studio SDK. You can download it there:
* [Visual Studio 2010 SDK](http://www.microsoft.com/download/en/details.aspx?id=2680) - if you HAVE NOT the installed Service Pack 1 for the Visual Studio 2010.
* [Visual Studio 2010 SP1 SDK](http://www.microsoft.com/download/en/details.aspx?id=21835) - if you HAVE the installed Service Pack 1 for the Visual Studio 2010.
* [Visual Studio 11 Beta SDK](http://www.microsoft.com/en-us/download/details.aspx?id=28990) - if you develop for Visual Studio 11
* [Microsoft F#](http://www.microsoft.com/en-us/download/details.aspx?id=11100) - Visual Studio integration for F# development
