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

## Notes About PlantUML

The solution contents are currently output to the PlantUML format, which is associated with the .puml file extension. Although diagrams for .puml files can be generated directly upon the PlantUML website, that approach might not be the most advisable at this time due to a combination of an extreme amount of spam on every page of their project website and the fact that their own instance of the translator has a small file size limit in place.

A useful and reliable alternative method for generating diagrams from PlantUML syntax, which also uses a local, integrated PlantUML.jar by default, is to install the PlantUML extension for Visual Studio Code (VSCode) published by jebbs. I can attest that at the time of this writing, his extension has been working without fail on fairly large project files.

If you wish to use an updated version of PlantUML.jar from the **Releases** tab of the [PlantUML Repository](https://github.com/plantuml/plantuml) on the VS Code extension, you can download and follow this general process once each time you want to upgrade.

-   Open the current release page.
-   Download **PlantUML.jar**. If questioned whether to Keep or Delete the file, click **Keep**.
-   In your **Downloads** folder, right-click the file, and from the context menu, select **Properties**.
-   In the **Attributes** group, click **Unblock**.
-   Click **OK**.
-   Move the .jar file to a useful location.
-   In the PlantUML extension page, click the gear icon, then from the context menu, select **Settings**.
-   Scroll down to the setting **Plantuml: Jar** and enter the full path and filename of the downloaded .jar file.
-   Scroll down and make sure **Plantuml: Render** is set to **Local**.
-   Close and re-start Visual Studio Code.
-   Tip: One quick way to see if your updated .jar file is being used is to purposely cause a syntax error in your .puml file, then right-click and from the context menu, select **Preview Current Diagram**. When the exclamation symbol appears in the PlantUML Preview tab, click it to see the version along with a description of your error.

<p>&nbsp;</p>

## Cross Platform

This project has no other dependencies than the Roslyn compiler system, and can be used on Windows, Linux, or macOS.
