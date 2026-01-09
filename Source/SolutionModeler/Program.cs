/*
 * Copyright (c). 2026 Daniel Patterson, MCSD (danielanywhere).
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using static SolutionModeler.ProgramUtil;

namespace SolutionModeler
{
	//*-------------------------------------------------------------------------*
	//*	Program																																	*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Main instance of the SolutionModeler Diagram Generator application.
	/// </summary>
	public class Program
	{
		//*************************************************************************
		//*	Private																																*
		//*************************************************************************
		private IOrderedEnumerable<IGrouping<string, INamedTypeSymbol>>
			mNamespaceGroups = null;
		private string mOutputExtension = "";
		private string mOutputPath = "";
		private List<INamedTypeSymbol> mPublicTypes = null;
		private string mSolutionPath = "";
		ImmutableHashSet<ISymbol> mTypeSet = null;
		private bool mWaitAfterEnd = true;

		//*-----------------------------------------------------------------------*
		//* CollectNamedTypesRecursively																					*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Recursively collect named type members from the provided type.
		/// </summary>
		/// <param name="type">
		/// Reference to the type to inspect.
		/// </param>
		/// <param name="collector">
		/// Reference to the type collector.
		/// </param>
		private static void CollectNamedTypesRecursively(INamedTypeSymbol type,
			List<INamedTypeSymbol> collector)
		{
			collector.Add(type);

			foreach(INamedTypeSymbol nested in type.GetTypeMembers())
			{
				CollectNamedTypesRecursively(nested, collector);
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* CollectTypes																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Collect all types from the provided namespace.
		/// </summary>
		/// <param name="ns">
		/// Reference to the namespace symbol to review.
		/// </param>
		/// <param name="collector">
		/// Reference to the type collector into which all of the found types will
		/// be stuffed.
		/// </param>
		private static void CollectTypes(INamespaceSymbol ns,
			List<INamedTypeSymbol> collector)
		{
			foreach(INamespaceOrTypeSymbol member in ns.GetMembers())
			{
				if(member is INamespaceSymbol childNs)
				{
					CollectTypes(childNs, collector);
				}
				else if(member is INamedTypeSymbol type)
				{
					CollectNamedTypesRecursively(type, collector);
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetPlantUmlTypeName																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the type name as it will be displayed in PlantUml.
		/// </summary>
		/// <param name="type">
		/// Reference to the type name to inspect.
		/// </param>
		/// <returns>
		/// The name of the printable PlantUml type.
		/// </returns>
		private static string GetPlantUmlTypeName(INamedTypeSymbol type)
		{
			string fullName =
				type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			return RightOf(fullName.Replace("global::", ""), ".");
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetSimpleTypeName																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the simple type name corresponding to the supplied type symbol.
		/// </summary>
		/// <param name="type">
		/// Reference to the type symbol to be inspected.
		/// </param>
		/// <returns>
		/// The simple, printable type name.
		/// </returns>
		private static string GetSimpleTypeName(ITypeSymbol type)
		{
			return type.ToDisplayString(
				SymbolDisplayFormat.MinimallyQualifiedFormat);
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetUnderlyingType																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the type underlying the specified type.
		/// </summary>
		/// <param name="type">
		/// Reference to the type to inspect.
		/// </param>
		/// <returns>
		/// The type, if any, underlying the supplied type.
		/// </returns>
		private static ITypeSymbol GetUnderlyingType(ITypeSymbol type)
		{
			ITypeSymbol result = type;

			if(type is INamedTypeSymbol named && named.TypeArguments.Length == 1)
			{
				result = named.TypeArguments[0];
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* ProcessNamespaces																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Process the namespaces for the open projects.
		/// </summary>
		private void ProcessNamespaces()
		{
			StringBuilder builder = new StringBuilder();
			string parameters = "";
			ITypeSymbol propType = null;
			HashSet<string> relations = new HashSet<string>();
			string typeName = "";

			foreach(IGrouping<string, INamedTypeSymbol> group in mNamespaceGroups)
			{
				Clear(builder);
				builder.AppendLine("@startuml");
				builder.AppendLine("set namespaceSeparator none");
				builder.AppendLine($"package \"{group.Key}\" {{");
				foreach(INamedTypeSymbol type in group)
				{
					typeName = GetPlantUmlTypeName(type);

					builder.AppendLine($"  class {typeName} {{");

					foreach(IPropertySymbol prop in
						type.GetMembers().OfType<IPropertySymbol>())
					{
						if(prop.DeclaredAccessibility == Accessibility.Public)
						{
							builder.AppendLine(
								$"    + {prop.Name} : {GetSimpleTypeName(prop.Type)}");
						}
					}
					foreach(IMethodSymbol method in
						type.GetMembers().OfType<IMethodSymbol>())
					{
						if(method.MethodKind == MethodKind.Ordinary &&
							method.DeclaredAccessibility == Accessibility.Public &&
							!method.IsImplicitlyDeclared &&
							!method.IsOverride)
						{
							parameters = string.Join(", ",
								method.Parameters.Select(p =>
									$"{GetSimpleTypeName(p.Type)} {p.Name}"));

							builder.AppendLine(
								$"    + {method.Name}({parameters}) : " +
								$"{GetSimpleTypeName(method.ReturnType)}");
						}
					}
					builder.AppendLine("  }");
				}
				builder.AppendLine("}");
				// Emit relationships
				relations.Clear();
				//foreach(INamedTypeSymbol type in mPublicTypes)
				foreach(INamedTypeSymbol type in group)
				{
					typeName = GetPlantUmlTypeName(type);
					if(type.BaseType is
						{ SpecialType: not SpecialType.System_Object } baseType &&
						mTypeSet.Contains(baseType))
					{
						relations.Add($"{GetPlantUmlTypeName(baseType)} <|-- {typeName}");
					}
					foreach(INamedTypeSymbol iface in type.AllInterfaces)
					{
						if(mTypeSet.Contains(iface))
						{
							relations.Add($"{GetPlantUmlTypeName(iface)} <|.. {typeName}");
						}
					}
					foreach(IPropertySymbol prop in
						type.GetMembers().OfType<IPropertySymbol>())
					{
						if(prop.DeclaredAccessibility == Accessibility.Public)
						{
							propType = GetUnderlyingType(prop.Type);
							if(propType is INamedTypeSymbol named &&
									mTypeSet.Contains(named) &&
									!SymbolEqualityComparer.Default.Equals(named, type))
							{
								relations.Add($"{typeName} --> {GetPlantUmlTypeName(named)}");
							}
						}
					}
				}
				foreach(string rel in relations.OrderBy(r => r))
				{
					builder.AppendLine(rel);
				}
				builder.AppendLine("@enduml");
				WritePackageFile(builder.ToString(), group.Key);
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* WritePackageFile																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Write the contents of the file to the output file with the specified
		/// suffix.
		/// </summary>
		/// <param name="content">
		/// The content to write to the file.
		/// </param>
		/// <param name="packageSuffix">
		/// The suffix to add to the filename.
		/// </param>
		private void WritePackageFile(string content, string packageSuffix)
		{
			string filename = "";
			string suffix = packageSuffix;

			if(mOutputPath?.Length > 0 && mOutputExtension?.Length > 0)
			{
				Directory.CreateDirectory(Path.GetDirectoryName(mOutputPath)!);
				if(packageSuffix?.Length > 0)
				{
					suffix = packageSuffix.Replace('.', '-');
					filename = $"{mOutputPath}-{suffix}{mOutputExtension}";
				}
				else
				{
					filename = $"{mOutputPath}{mOutputExtension}";
				}
				File.WriteAllText(filename, content, Encoding.UTF8);
				Console.WriteLine(
					$"PlantUML diagram written to: {Path.GetFileName(filename)}");
			}
		}
		//*-----------------------------------------------------------------------*

		//*************************************************************************
		//*	Protected																															*
		//*************************************************************************
		//*************************************************************************
		//*	Public																																*
		//*************************************************************************
		//*-----------------------------------------------------------------------*
		//*	_Main																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Configure and run the application.
		/// </summary>
		public static async Task Main(string[] args)
		{
			FileInfo file = null;
			Program prg = null;

			if(args.Length >= 2)
			{
				prg = new Program();
				prg.mSolutionPath = Path.GetFullPath(args[0]);
				if(File.Exists(prg.mSolutionPath))
				{
					file = new FileInfo(Path.GetFullPath(args[1]));
					prg.mOutputPath = Path.Combine(file.Directory.FullName,
						Path.GetFileNameWithoutExtension(file.Name));
					prg.mOutputExtension = file.Extension;
					if(!prg.mOutputExtension.StartsWith('.'))
					{
						prg.mOutputExtension = "." + prg.mOutputExtension;
					}
					await prg.Run();
					if(prg.mWaitAfterEnd)
					{
						Console.WriteLine("Press [Enter] to exit...");
						Console.ReadLine();
					}
				}
				else
				{
					Console.WriteLine($"Solution not found: {prg.mSolutionPath}");
				}
			}
			else
			{
				Console.WriteLine(
					"Usage: SolutionModeler <sln-filename> <puml-filename>");
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Run																																		*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Run the configured application.
		/// </summary>
		public async Task Run()
		{
			List<INamedTypeSymbol> allTypes = null;
			StringBuilder builder = new StringBuilder();
			Compilation compilation = null;
			Solution solution = null;

			if(!MSBuildLocator.IsRegistered)
			{
				MSBuildLocator.RegisterDefaults();
			}

			using(MSBuildWorkspace workspace = MSBuildWorkspace.Create())
			{
				Console.WriteLine($"Loading solution: {mSolutionPath}");
				solution = await workspace.OpenSolutionAsync(mSolutionPath);
				allTypes = new List<INamedTypeSymbol>();

				//	Enumerate the projects.
				foreach(Project project in solution.Projects)
				{
					if(!project.Name.Contains("Test",
							StringComparison.OrdinalIgnoreCase) &&
						!project.Name.Contains("Example",
							StringComparison.OrdinalIgnoreCase) &&
						!project.Name.Contains("Sample",
							StringComparison.OrdinalIgnoreCase))
					{
						Console.WriteLine($"Processing project: {project.Name}");
						compilation = await project.GetCompilationAsync();
						if(compilation != null)
						{
							CollectTypes(compilation.Assembly.GlobalNamespace, allTypes);
						}
					}
				}

				//	Retrieve the public types.
				mPublicTypes = allTypes
					.Where(t =>
						t.DeclaredAccessibility == Accessibility.Public &&
						!t.IsImplicitlyDeclared &&
						t.TypeKind is TypeKind.Class
							or TypeKind.Interface
							or TypeKind.Struct
							or TypeKind.Enum
							or TypeKind.Delegate)
					.ToList();

				Console.WriteLine($"Public types found: {mPublicTypes.Count}");

				// Group types by namespace
				mNamespaceGroups = mPublicTypes
					.GroupBy(t => t.ContainingNamespace.ToDisplayString())
					.OrderBy(g => g.Key);

				mTypeSet = mPublicTypes.ToImmutableHashSet(
					SymbolEqualityComparer.Default);

				ProcessNamespaces();
			}
		}
		//*-----------------------------------------------------------------------*

	}
	//*-------------------------------------------------------------------------*

}
