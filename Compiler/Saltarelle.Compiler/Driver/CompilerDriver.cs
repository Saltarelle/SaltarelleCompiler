using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Minification;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.OOPEmulation;
using TopologicalSort;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.JSModel.SourceMaps;

namespace Saltarelle.Compiler.Driver {
	public class CompilerDriver {
		private class ErrorReporterWrapper : IErrorReporter {
			private readonly IErrorReporter _prev;

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

			public void AdditionalLocation(Location location) {
				_prev.AdditionalLocation(location);
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

		private readonly ErrorReporterWrapper _errorReporter;

		private static string GetAssemblyName(CompilerOptions options) {
			if (options.OutputAssemblyPath != null)
				return Path.GetFileNameWithoutExtension(options.OutputAssemblyPath);
			else if (options.SourceFiles.Count > 0)
				return Path.GetFileNameWithoutExtension(options.SourceFiles[0]);
			else
				return null;
		}

		private string ResolveReference(string filename, IEnumerable<string> paths) {
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
			_errorReporter.Location = Location.None;
			_errorReporter.Message(Messages._7997, filename);
			return null;
		}

		private SyntaxTree ParseSourceFile(string path, CSharpParseOptions options) {
			if (!File.Exists(path))
				_errorReporter.Message(DiagnosticSeverity.Error, "CS2001", "Source file `{0}' could not be found", path);

			try {
				using (var rdr = new StreamReader(path)) {
					return SyntaxFactory.ParseSyntaxTree(rdr.ReadToEnd(), options, path);
				}
			}
			catch (IOException ex) {
				_errorReporter.Message(DiagnosticSeverity.Error, "CS2001", "Error reading source file `{0}': {1}", path, ex.Message);
				return null;
			}
		}

		private CSharpCompilation CreateCompilation(CompilerOptions options) {
			var allPaths = options.AdditionalLibPaths.Concat(new[] { Environment.CurrentDirectory }).ToList();

			var compilationOptions = new CSharpCompilationOptions(
				outputKind:                    (options.HasEntryPoint ? OutputKind.WindowsApplication : OutputKind.DynamicallyLinkedLibrary),
				mainTypeName:                  options.EntryPointClass,
				warningLevel:                  options.WarningLevel,
				generalDiagnosticOption:       options.TreatWarningsAsErrors ? ReportDiagnostic.Error : ReportDiagnostic.Warn,
				specificDiagnosticOptions:             options.DisabledWarnings.Select(w => new KeyValuePair<string, ReportDiagnostic>(string.Format(CultureInfo.InvariantCulture, "CS{0:0000}", w), ReportDiagnostic.Suppress))
				                               .Concat(options.WarningsAsErrors.Select(w => new KeyValuePair<string, ReportDiagnostic>(string.Format(CultureInfo.InvariantCulture, "CS{0:0000}", w), ReportDiagnostic.Error)))
				                               .Concat(options.WarningsNotAsErrors.Select(w => new KeyValuePair<string, ReportDiagnostic>(string.Format(CultureInfo.InvariantCulture, "CS{0:0000}", w), ReportDiagnostic.Warn))),
				cryptoKeyFile:                 options.KeyFile,
				cryptoKeyContainer:            options.KeyContainer
			);

			var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp5, !string.IsNullOrEmpty(options.DocumentationFile) ? DocumentationMode.Diagnose : DocumentationMode.None, SourceCodeKind.Regular, options.DefineConstants);
			var syntaxTrees = options.SourceFiles.Select(s => ParseSourceFile(s, parseOptions)).Where(s => s != null).ToList();

			var references = new List<MetadataReference>();
			bool hasReferenceError = false;
			foreach (var r in options.References) {
				var path = ResolveReference(r.Filename, allPaths);
				if (path != null)
					references.Add(new MetadataFileReference(path, MetadataImageKind.Assembly, r.Alias != null ? ImmutableArray.Create(r.Alias) : ImmutableArray<string>.Empty));
				else
					hasReferenceError = true;
			}

			if (hasReferenceError)
				return null;

			return CSharpCompilation.Create(GetAssemblyName(options), syntaxTrees, references, compilationOptions);
		}

		public CompilerDriver(IErrorReporter errorReporter) {
			_errorReporter = new ErrorReporterWrapper(errorReporter);
		}

		private static IEnumerable<System.Reflection.Assembly> TopologicalSortPlugins(IList<Tuple<string, IList<string>, System.Reflection.Assembly>> references) {
			return TopologicalSorter.TopologicalSort(references, r => r.Item1, references.SelectMany(a => a.Item2, (a, r) => Edge.Create(a.Item1, r)))
			                        .Select(r => r.Item3)
			                        .Where(a => a != null);
		}

		private static readonly Type[] _pluginTypes = new[] { typeof(IJSTypeSystemRewriter), typeof(IMetadataImporter), typeof(IRuntimeLibrary), typeof(IOOPEmulator), typeof(ILinker), typeof(INamer), typeof(IAutomaticMetadataAttributeApplier) };

		private static void RegisterPlugin(IWindsorContainer container, System.Reflection.Assembly plugin) {
			container.Register(Classes.FromAssembly(plugin).Where(t => _pluginTypes.Any(pt => pt.IsAssignableFrom(t))).WithServiceSelect((t, _) => t.GetInterfaces().Intersect(_pluginTypes)));
		}

		private List<AssemblyResource> LoadResources(IEnumerable<EmbeddedResource> resources) {
			return resources.Select(r => new AssemblyResource(r.ResourceName, r.IsPublic, () => {
			                                 try {
			                                     return File.OpenRead(r.Filename);
			                                 }
			                                 catch (Exception ex) {
			                                     _errorReporter.Message(DiagnosticSeverity.Error, "CS1566", "Error reading resource file " + r.Filename + ": " + ex.Message);
			                                     return new MemoryStream();
			                                 }
			                             }))
			                .ToList();
		}

		private void PerformMetadataWriteback(Stream stream, Compilation compilation, IMetadataImporter metadataImporter) {
			stream.Seek(0, SeekOrigin.Begin);
			var asm = AssemblyDefinition.ReadAssembly(stream);
			ReferenceMetadataImporter.Write(compilation, asm, metadataImporter);
			stream.Seek(0, SeekOrigin.Begin);
			asm.Write(stream);
			stream.Seek(0, SeekOrigin.Begin);
		}

		public bool Compile(CompilerOptions options) {
			try {
				var compilation = CreateCompilation(options);
				if (compilation == null)
					return false;

				if (!options.AlreadyCompiled) {
					foreach (var d in compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Warning || d.Severity == DiagnosticSeverity.Error)) {
						var severity = d.IsWarningAsError ? DiagnosticSeverity.Error : d.Severity;
						_errorReporter.Location = d.Location;
						_errorReporter.Message(severity, d.Id, d.GetMessage());
						foreach (var l in d.AdditionalLocations)
							_errorReporter.AdditionalLocation(l);
					}
					if (_errorReporter.HasErrors)
						return false;
				}

				var references = LoadReferences(compilation.References.Select(r => r.Display));
				if (references == null)
					return false;

				var resources = LoadResources(options.EmbeddedResources);

				var container = new WindsorContainer();
				foreach (var plugin in TopologicalSortPlugins(references).Reverse())
					RegisterPlugin(container, plugin);

				foreach (var p in options.Plugins) {
					var plugin = LoadPlugin(Path.GetFullPath(p));
					if (plugin != null)
						RegisterPlugin(container, plugin);
				}
				if (_errorReporter.HasErrors)
					return false;

				var referenceMetadataImporter = new ReferenceMetadataImporter(_errorReporter);

				container.Register(Component.For<IErrorReporter>().Instance(_errorReporter),
				                   Component.For<CompilerOptions>().Instance(options),
				                   Component.For<IAttributeStore>().UsingFactoryMethod(k => new AttributeStore(k.Resolve<Compilation>(), k.Resolve<IErrorReporter>(), k.ResolveAll<IAutomaticMetadataAttributeApplier>())),
				                   Component.For<Compilation>().Instance(compilation),
				                   Component.For<IMetadataImporter>().Instance(referenceMetadataImporter),
				                   Component.For<ICompiler>().ImplementedBy<Compiler.Compiler>()
				                  );

				container.Resolve<IMetadataImporter>().Prepare(compilation.Assembly.GetAllTypes());
				if (_errorReporter.HasErrors)
					return false;

				var compiledTypes = container.Resolve<ICompiler>().Compile(compilation);
				if (_errorReporter.HasErrors)
					return false;

				foreach (var rewriter in container.ResolveAll<IJSTypeSystemRewriter>())
					compiledTypes = rewriter.Rewrite(compiledTypes);
				if (_errorReporter.HasErrors)
					return false;

				var invoker = new OOPEmulatorInvoker(container.Resolve<IOOPEmulator>(), container.Resolve<IMetadataImporter>(), container.Resolve<IErrorReporter>());
				var js = invoker.Process(compiledTypes.ToList(), compilation.GetEntryPoint(CancellationToken.None), resources);
				if (_errorReporter.HasErrors)
					return false;

				js = container.Resolve<ILinker>().Process(js);
				if (_errorReporter.HasErrors)
					return false;

				string outputAssemblyPath = !string.IsNullOrEmpty(options.OutputAssemblyPath) ? options.OutputAssemblyPath : Path.ChangeExtension(options.SourceFiles[0], ".dll");
				string outputScriptPath   = !string.IsNullOrEmpty(options.OutputScriptPath)   ? options.OutputScriptPath   : Path.ChangeExtension(options.SourceFiles[0], ".js");
				string sourceMapPath      = outputScriptPath + ".map";

				if (options.AlreadyCompiled) {
					using (Stream assemblyStream = File.Open(outputAssemblyPath, FileMode.Open, FileAccess.ReadWrite)) {
						PerformMetadataWriteback(assemblyStream, compilation, container.Resolve<IMetadataImporter>());
					}
				}
				else {
					try {
						using (Stream assemblyStream = new MemoryStream(),
						              docStream      = !string.IsNullOrEmpty(options.DocumentationFile) ? File.OpenWrite(options.DocumentationFile) : null)
						{
							compilation.Emit(assemblyStream, null, null, null, docStream, null, resources.Select(r => new ResourceDescription(r.Name, r.GetResourceStream, r.IsPublic)));
							PerformMetadataWriteback(assemblyStream, compilation, container.Resolve<IMetadataImporter>());
							using (var assemblyFile = File.OpenWrite(outputAssemblyPath)) {
								assemblyStream.CopyTo(assemblyFile);
							}
						}
					}
					catch (IOException ex) {
						_errorReporter.Location = Location.None;
						_errorReporter.Message(Messages._7950, ex.Message);
						return false;
					}
				}

				if (options.MinimizeScript) {
					js = ((JsBlockStatement)Minifier.Process(JsStatement.Block(js))).Statements;
				}

				var mapGenerator = new SourceMapsGenerator(outputScriptPath, sourceMapPath); 

				string script = options.MinimizeScript ? OutputFormatter.FormatMinified(js, mapGenerator) : OutputFormatter.Format(js, mapGenerator);
				try {
					using (var writer = new StreamWriter(outputScriptPath)) {
						writer.Write(script);
						writer.WriteLine();
						writer.WriteLine("//# sourceMappingURL=" + Path.GetFileName(sourceMapPath));
					}
				}
				catch (IOException ex) {
					_errorReporter.Location = Location.None;
					_errorReporter.Message(Messages._7951, ex.Message);
					return false;
				}

				try {
					using (var mapstream = new StreamWriter(sourceMapPath)) {
						mapGenerator.WriteSourceMap(mapstream);
					}
				}
				catch (IOException ex) {
					_errorReporter.Location = Location.None;
					_errorReporter.Message(Messages._7952, ex.Message);
					return false;
				}

				return true;
			}
			catch (Exception ex) {
				_errorReporter.Location = Location.None;
				_errorReporter.InternalError(ex);
				return false;
			}
		}

		private static System.Reflection.Assembly LoadPlugin(AssemblyDefinition def) {
			foreach (var r in def.Modules.SelectMany(m => m.Resources).OfType<Mono.Cecil.EmbeddedResource>()) {
				if (r.Name.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)) {
					var data = r.GetResourceData();
					var asm = AssemblyDefinition.ReadAssembly(new MemoryStream(data));

					var result = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == asm.Name.Name);
					if (result == null)
						result = System.Reflection.Assembly.Load(data);
					return result;
				}
			}
			return null;
		}

		private System.Reflection.Assembly LoadPlugin(string path) {
			try {
				return System.Reflection.Assembly.LoadFile(path);
			}
			catch (Exception ex) {
				_errorReporter.Location = Location.None;
				_errorReporter.Message(Messages._7994, path, ex.Message);
				return null;
			}
		}

		private static IList<string> GetReferencedAssemblyNames(AssemblyDefinition asm) {
			return asm.Modules.SelectMany(m => m.AssemblyReferences, (_, r) => r.FullName).Distinct().ToList();
		}

		private IList<Tuple<string, IList<string>, System.Reflection.Assembly>> LoadReferences(IEnumerable<string> references) {
			var assemblies = references.Select(AssemblyDefinition.ReadAssembly).ToList(); // Shouldn't result in errors because Roslyn would have caught it.

			var indirectReferences = (  from a in assemblies
			                            from m in a.Modules
			                            from r in m.AssemblyReferences
			                          select r.FullName)
			                         .Distinct();

			var directReferences = from a in assemblies select a.FullName;

			var missingReferences = indirectReferences.Except(directReferences).ToList();

			if (missingReferences.Count > 0) {
				_errorReporter.Location = Location.None;
				foreach (var r in missingReferences)
					_errorReporter.Message(Messages._7996, r);
				return null;
			}

			return assemblies.Select(asm => Tuple.Create(asm.FullName, GetReferencedAssemblyNames(asm), LoadPlugin(asm))).ToList();
		}
	}
}
