using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Minification;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.OOPEmulation;
using TopologicalSort;
using Component = Castle.MicroKernel.Registration.Component;
using Saltarelle.Compiler.Roslyn;

namespace Saltarelle.Compiler.Driver {
	public class CompilerDriver {
		private readonly IErrorReporter _errorReporter;

		private static string GetAssemblyName(CompilerOptions options) {
			if (options.OutputAssemblyPath != null)
				return Path.GetFileNameWithoutExtension(options.OutputAssemblyPath);
			else if (options.SourceFiles.Count > 0)
				return Path.GetFileNameWithoutExtension(options.SourceFiles[0]);
			else
				return null;
		}

		private static string ResolveReference(string filename, IEnumerable<string> paths, IErrorReporter er) {
			// Code taken from mcs, so it should match that behavior.
			bool? hasExtension = null;
			foreach (var path in paths) {
				var file = Path.Combine(path, filename);

				if (!File.Exists(file)) {
					if (!hasExtension.HasValue)
						hasExtension = filename.EndsWith(".dll", StringComparison.Ordinal) || filename.EndsWith(".exe", StringComparison.Ordinal);

					if (hasExtension.Value)
						continue;

					file += ".dll";
					if (!File.Exists(file))
						continue;
				}

				return Path.GetFullPath(file);
			}
			er.Location = Location.None;
			er.Message(Messages._7997, filename);
			return null;
		}

		private static CSharpCompilation CreateCompilation(CompilerOptions options, IErrorReporter errorReporter) {
			var allPaths = options.AdditionalLibPaths.Concat(new[] { Environment.CurrentDirectory }).ToList();

			var compilationOptions = new CSharpCompilationOptions(
				outputKind:                    (options.HasEntryPoint ? OutputKind.WindowsApplication : OutputKind.DynamicallyLinkedLibrary),
				mainTypeName:                  options.EntryPointClass,
				warningLevel:                  options.WarningLevel,
				generalDiagnosticOption:       options.TreatWarningsAsErrors ? ReportDiagnostic.Error : ReportDiagnostic.Warn
			);

			foreach (var w in options.DisabledWarnings)
				compilationOptions.SpecificDiagnosticOptions.Add(w.ToString(CultureInfo.InvariantCulture), ReportDiagnostic.Suppress);
			foreach (var w in options.WarningsAsErrors)
				compilationOptions.SpecificDiagnosticOptions.Add(w.ToString(CultureInfo.InvariantCulture), ReportDiagnostic.Error);
			foreach (var w in options.WarningsNotAsErrors)
				compilationOptions.SpecificDiagnosticOptions.Add(w.ToString(CultureInfo.InvariantCulture), ReportDiagnostic.Warn);

			var syntaxTrees = options.SourceFiles.Select(s => { 
			                                        using (var rdr = new StreamReader(s)) {
			                                            return SyntaxFactory.ParseSyntaxTree(rdr.ReadToEnd(), new CSharpParseOptions(LanguageVersion.CSharp5, DocumentationMode.Diagnose, SourceCodeKind.Regular, options.DefineConstants), s);
			                                        }
			                                    }).ToList();

			var references = new List<MetadataReference>();
			bool hasReferenceError = false;
			foreach (var r in options.References) {
				var path = ResolveReference(r.Filename, allPaths, errorReporter);
				if (path != null)
					references.Add(new MetadataFileReference(path, MetadataImageKind.Assembly, r.Alias != null ? ImmutableArray.Create(r.Alias) : ImmutableArray<string>.Empty));
				else
					hasReferenceError = true;
			}

			if (hasReferenceError)
				return null;

			#warning TODO: Verify that aliases work
			return CSharpCompilation.Create(GetAssemblyName(options), syntaxTrees, references, compilationOptions);
		}

		public CompilerDriver(IErrorReporter errorReporter) {
			_errorReporter = errorReporter;
		}

		private static IEnumerable<System.Reflection.Assembly> TopologicalSortPlugins(IList<Tuple<IUnresolvedAssembly, IList<string>, System.Reflection.Assembly>> references) {
			return TopologicalSorter.TopologicalSort(references, r => r.Item1.AssemblyName, references.SelectMany(a => a.Item2, (a, r) => Edge.Create(a.Item1.AssemblyName, r)))
			                        .Select(r => r.Item3)
			                        .Where(a => a != null);
		}

		private static readonly Type[] _pluginTypes = new[] { typeof(IJSTypeSystemRewriter), typeof(IMetadataImporter), typeof(IRuntimeLibrary), typeof(IOOPEmulator), typeof(ILinker), typeof(INamer), typeof(IAutomaticMetadataAttributeApplier) };

		private static void RegisterPlugin(IWindsorContainer container, System.Reflection.Assembly plugin) {
			container.Register(Classes.FromAssembly(plugin).Where(t => _pluginTypes.Any(pt => pt.IsAssignableFrom(t))).WithServiceSelect((t, _) => t.GetInterfaces().Intersect(_pluginTypes)));
		}

		#warning TODO!!!!!
		private static IEnumerable<AssemblyResource> LoadResources(IEnumerable<EmbeddedResource> resources) {
			return resources.Select(r => new AssemblyResource(r.ResourceName, r.IsPublic, () => File.OpenRead(r.Filename)));
		}

		private void InitializeAttributeStore(AttributeStore attributeStore, WindsorContainer container, Compilation compilation) {
			var references = compilation.References.ToList();
			var types = compilation.GetAllTypes().ToList();
			foreach (var applier in container.ResolveAll<IAutomaticMetadataAttributeApplier>()) {
				applier.Process(compilation.Assembly);
				foreach (var a in references)
					applier.Process((IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(a));
				foreach (var t in types)
					applier.Process(t);
			}
			attributeStore.RunAttributeCode();
		}

		private class ErrorReporterWrapper : IErrorReporter {
			private IErrorReporter _prev;

			public bool HasErrors { get; private set; }

			public ErrorReporterWrapper(IErrorReporter prev) {
				_prev = prev;
			}

			public Location Location { get { return _prev.Location; } set { _prev.Location = value; } }

			public void Message(DiagnosticSeverity severity, string code, string message, params object[] args) {
				_prev.Message(severity, code, message, args);
				if (severity == DiagnosticSeverity.Error)
					HasErrors = true;
			}

			public void InternalError(string text) {
				_prev.InternalError(text);
				HasErrors = true;
			}

			public void InternalError(Exception ex, string additionalText = null) {
				_prev.InternalError(ex, additionalText);
				HasErrors = true;
			}
		}

		public bool Compile(CompilerOptions options) {
			//string intermediateAssemblyFile = Path.GetTempFileName(), intermediateDocFile = Path.GetTempFileName();
			//var actualOut = Console.Out;
			try {
				var errorReporter = new ErrorReporterWrapper(_errorReporter);
				//Console.SetOut(new StringWriter());	// I don't trust the third-party libs to not generate spurious random messages, so make sure that any of those messages are suppressed.

				var compilation = CreateCompilation(options, _errorReporter);
				if (compilation == null)
					return false;

				if (!options.AlreadyCompiled) {
					bool hasError = false;
					foreach (var d in compilation.GetDiagnostics()) {
						#warning TODO: Additional locations
						errorReporter.Message(d.Severity, d.Id, d.GetMessage());
						if (d.Severity == DiagnosticSeverity.Error)
							hasError = true;
					}
					if (hasError)
						return false;
				}

				//var references = LoadReferences(compilation.References.Select(r => r.Display), errorReporter);
				//if (references == null)
				//	return false;
				//
				//PreparedCompilation compilation = PreparedCompilation.CreateCompilation(settings.AssemblyName, options.SourceFiles.Select(f => new SimpleSourceFile(f, settings.Encoding)), references.Select(r => r.Item1), options.DefineConstants, LoadResources(options.EmbeddedResources));
				//
				//IMethodSymbol entryPoint = FindEntryPoint(options, er, compilation);

				var container = new WindsorContainer();
				foreach (var plugin in TopologicalSortPlugins(references).Reverse())
					RegisterPlugin(container, plugin);

				var attributeStore = new AttributeStore(compilation, errorReporter);

				container.Register(Component.For<IErrorReporter>().Instance(er),
				                   Component.For<CompilerOptions>().Instance(options),
				                   Component.For<IAttributeStore>().Instance(attributeStore),
				                   Component.For<ICompilation>().Instance(compilation.Compilation),
				                   Component.For<ICompiler>().ImplementedBy<Compiler.Compiler>()
				                  );

				InitializeAttributeStore(attributeStore, container, compilation);

				container.Resolve<IMetadataImporter>().Prepare(compilation.GetAllTypes());

				var compiledTypes = container.Resolve<ICompiler>().Compile(compilation);

				foreach (var rewriter in container.ResolveAll<IJSTypeSystemRewriter>())
					compiledTypes = rewriter.Rewrite(compiledTypes);

				var invoker = new OOPEmulatorInvoker(container.Resolve<IOOPEmulator>(), container.Resolve<IMetadataImporter>(), container.Resolve<IErrorReporter>());

				var js = invoker.Process(compiledTypes.ToList(), compilation.GetEntryPoint());
				js = container.Resolve<ILinker>().Process(js);

				if (errorReporter.HasErrors)
					return false;

				string outputAssemblyPath = !string.IsNullOrEmpty(options.OutputAssemblyPath) ? options.OutputAssemblyPath : Path.ChangeExtension(options.SourceFiles[0], ".dll");
				string outputScriptPath   = !string.IsNullOrEmpty(options.OutputScriptPath)   ? options.OutputScriptPath   : Path.ChangeExtension(options.SourceFiles[0], ".js");

				if (!options.AlreadyCompiled) {
					try {
						File.Copy(intermediateAssemblyFile, outputAssemblyPath, true);
					}
					catch (IOException ex) {
						er.Region = FileLinePositionSpan.Empty;
						er.Message(Messages._7950, ex.Message);
						return false;
					}
					if (!string.IsNullOrEmpty(options.DocumentationFile)) {
						try {
							File.Copy(intermediateDocFile, options.DocumentationFile, true);
						}
						catch (IOException ex) {
							er.Region = FileLinePositionSpan.Empty;
							er.Message(Messages._7952, ex.Message);
							return false;
						}
					}
				}

				if (options.MinimizeScript) {
					js = ((JsBlockStatement)Minifier.Process(JsStatement.Block(js))).Statements;
				}

				string script = options.MinimizeScript ? OutputFormatter.FormatMinified(js) : OutputFormatter.Format(js);
				try {
					File.WriteAllText(outputScriptPath, script, settings.Encoding);
				}
				catch (IOException ex) {
					er.Region = FileLinePositionSpan.Empty;
					er.Message(Messages._7951, ex.Message);
					return false;
				}
				return true;
			}
			catch (Exception ex) {
				er.Region = FileLinePositionSpan.Empty;
				er.InternalError(ex.ToString());
				return false;
			}
			finally {
				if (!options.AlreadyCompiled) {
					try { File.Delete(intermediateAssemblyFile); } catch {}
					try { File.Delete(intermediateDocFile); } catch {}
				}
				if (actualOut != null) {
					Console.SetOut(actualOut);
				}
			}
		}

		private static System.Reflection.Assembly LoadPlugin(IKVM.Reflection.Assembly asm) {
			foreach (var name in asm.GetManifestResourceNames()) {
				if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) {
					using (var strm = asm.GetManifestResourceStream(name))
					using (var ms = new MemoryStream())
					using (var uni = new IKVM.Reflection.Universe()) {
						strm.CopyTo(ms);
						ms.Position = 0;
						string referenceName = uni.LoadAssembly(uni.OpenRawModule(ms, name)).GetName().Name;
						var result = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == referenceName);
						if (result == null)
							result = System.Reflection.Assembly.Load(ms.ToArray());
						return result;
					}
				}
			}

			return null;
		}

		private static IEnumerable<string> GetReferencedAssemblyNames(IKVM.Reflection.Assembly asm) {
			return asm.GetReferencedAssemblies().Select(r => r.Name);
		}

		private static IList<Tuple<IUnresolvedAssembly, IList<string>, System.Reflection.Assembly>> LoadReferences(IEnumerable<string> references, IErrorReporter er) {
			using (var universe = new IKVM.Reflection.Universe(IKVM.Reflection.UniverseOptions.DisablePseudoCustomAttributeRetrieval | IKVM.Reflection.UniverseOptions.SupressReferenceTypeIdentityConversion)) {
				var assemblies = references.Select(universe.LoadFile).ToList();
				var indirectReferences = assemblies.SelectMany(GetReferencedAssemblyNames).Distinct();
				var directReferences = from a in assemblies select a.GetName().Name;
				var missingReferences = indirectReferences.Except(directReferences).ToList();

				if (missingReferences.Count > 0) {
					er.Region = FileLinePositionSpan.Empty;
					foreach (var r in missingReferences)
						er.Message(Messages._7996, r);
					return null;
				}

				return assemblies.Select(asm => Tuple.Create(new IkvmLoader { IncludeInternalMembers = true }.LoadAssembly(asm), (IList<string>)GetReferencedAssemblyNames(asm).ToList(), LoadPlugin(asm))).ToList();
			}
		}
#endif	
	}
}
