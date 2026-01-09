# SolutionModeler

This is a .NET Core command-line application using the native .NET Roslyn compiler to create PlantUml object model diagrams from the projects in any .NET Solution (.sln) file.

It takes two parameters. The first is the fully qualified path and filename of the .NET Solution file, and the second is the fully qualified path and filename of the base output filename (.puml file).

```plaintext
SolutionModeler <sln-filename> <puml-filename>

```

<p>&nbsp;</p>

## Multiple Output Files

This version of the application, written specifically to generate quick documentation about [ShapeCrawler/ShapeCrawler](https://github.com/ShapeCrawler/ShapeCrawler), prepares a separate file for each namespace, specifically to handle scenarios where there are several objects and potentially several namespaces.

As you can see in the **Examples/ShapeCrawler** folder of this repository, eight separate files were created from the following command-line, where only a base filename was specified.

```bash
SolutionModeler C:\Develop\GitHub\ShapeCrawler\ShapeCrawler.sln C:\Develop\Shared\SolutionModeler\Examples\ShapeCrawler\ShapeCrawler.puml

```

<p>&nbsp;</p>

## Cross Platform

This project has no other dependencies than the Roslyn compiler system, and can be used on Windows, Linux, or macOS.
