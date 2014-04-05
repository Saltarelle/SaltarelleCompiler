using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using ICSharpCode.NRefactory.TypeSystem;
using System.Linq;
using Mono.CSharp;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Minification;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.OOPEmulation;
using TopologicalSort;
using Component = Castle.MicroKernel.Registration.Component;

namespace Saltarelle.Compiler.Driver {
	public class CompilerDriver {
		private class Resource : IAssemblyResource {
			public string Name { get; private set; }
			public AssemblyResourceType Type { get { return AssemblyResourceType.Embedded; } }
			public string LinkedFileName { get { return null; } }
			public bool IsPublic { get; private set; }
			private readonly string _filename;

			public Stream GetResourceStream() {
				return File.OpenRead(_filename);
			}

			public Resource(string name, string filename, bool isPublic) {
				Name      = name;
				_filename = filename;
				IsPublic  = isPublic;
			}
		}

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
			er.Region = DomRegion.Empty;
			er.Message(Messages._7997, filename);
			return null;
		}

		private static CompilerSettings MapSettings(CompilerOptions options, string outputAssemblyPath, string outputDocFilePath, IErrorReporter er) {
			var allPaths = options.AdditionalLibPaths.Concat(new[] { Environment.CurrentDirectory }).ToList();

			var result = new CompilerSettings {
				Target                    = (options.HasEntryPoint ? Target.Exe : Target.Library),
				Platform                  = Platform.AnyCPU,
				TargetExt                 = (options.HasEntryPoint ? ".exe" : ".dll"),
				MainClass                 = options.EntryPointClass,
				VerifyClsCompliance       = false,
				Optimize                  = false,
				Version                   = LanguageVersion.V_5,
				EnhancedWarnings          = false,
				LoadDefaultReferences     = false,
				TabSize                   = 1,
				WarningsAreErrors         = options.TreatWarningsAsErrors,
				FatalCounter              = 100,
				WarningLevel              = options.WarningLevel,
				Encoding                  = Encoding.UTF8,
				DocumentationFile         = !string.IsNullOrEmpty(options.DocumentationFile) ? outputDocFilePath : null,
				OutputFile                = outputAssemblyPath,
				AssemblyName              = GetAssemblyName(options),
				StdLib                    = false,
				StdLibRuntimeVersion      = RuntimeVersion.v4,
				StrongNameKeyContainer    = options.KeyContainer,
				StrongNameKeyFile         = options.KeyFile,
			};
			result.SourceFiles.AddRange(options.SourceFiles.Select((f, i) => new SourceFile(f, f, i + 1)));
			foreach (var r in options.References) {
				string resolvedPath = ResolveReference(r.Filename, allPaths, er);
				if (r.Alias == null)
					result.AssemblyReferences.Add(resolvedPath);
				else
					result.AssemblyReferencesAliases.Add(Tuple.Create(r.Alias, resolvedPath));
			}
			foreach (var c in options.DefineConstants)
				result.AddConditionalSymbol(c);
			foreach (var w in options.DisabledWarnings)
				result.SetIgnoreWarning(w);
			foreach (var w in options.WarningsAsErrors)
				result.AddWarningAsError(w);
			foreach (var w in options.WarningsNotAsErrors)
				result.AddWarningOnly(w);
			if (options.EmbeddedResources.Count > 0)
				result.Resources = options.EmbeddedResources.Select(r => new AssemblyResource(r.Filename, r.ResourceName, isPrivate: !r.IsPublic) { IsEmbeded = true }).ToList();

			if (result.AssemblyReferencesAliases.Count > 0) {	// NRefactory does currently not support reference aliases, this check will hopefully go away in the future.
				er.Region = DomRegion.Empty;
				er.Message(Messages._7998, "aliased reference");
			}

			return result;
		}

		private class ConvertingReportPrinter : ReportPrinter {
			private readonly IErrorReporter _errorReporter;

			public ConvertingReportPrinter(IErrorReporter errorReporter) {
				_errorReporter = errorReporter;
			}

			public override void Print(AbstractMessage msg, bool showFullPath) {
				base.Print(msg, showFullPath);
				_errorReporter.Region = new DomRegion(msg.Location.NameFullPath, msg.Location.Row, msg.Location.Column, msg.Location.Row, msg.Location.Column);
				_errorReporter.Message(msg.IsWarning ? MessageSeverity.Warning : MessageSeverity.Error, msg.Code, msg.Text.Replace("{", "{{").Replace("}", "}}"));
			}
		}

		private class SimpleSourceFile : ISourceFile {
			private readonly Encoding _encoding;
			private readonly string _filename;

			public SimpleSourceFile(string filename, Encoding encoding) {
				_filename = filename;
				_encoding = encoding;
			}

			public string Filename {
				get { return _filename; }
			}

			public TextReader Open() {
				return new StreamReader(Filename, _encoding);
			}
		}

		public class ErrorReporterWrapper : IErrorReporter {
			private readonly IErrorReporter _er;
			private readonly TextWriter _actualConsoleOut;

			public bool HasErrors { get; private set; }

			public ErrorReporterWrapper(IErrorReporter er, TextWriter actualConsoleOut) {
				_er = er;
				_actualConsoleOut = actualConsoleOut;
			}

			private void WithActualOut(Action a) {
				TextWriter old = Console.Out;
				try {
					Console.SetOut(_actualConsoleOut);
					a();
				}
				finally {
					Console.SetOut(old);
				}
			}

			public DomRegion Region {
				get { return _er.Region; }
				set { _er.Region = value; }
			}

			public void Message(MessageSeverity severity, int code, string message, params object[] args) {
				WithActualOut(() => _er.Message(severity, code, message, args));
				if (severity == MessageSeverity.Error)
					HasErrors = true;
			}

			public void InternalError(string text) {
				WithActualOut(() => _er.InternalError(text));
				HasErrors = true;
			}

			public void InternalError(Exception ex, string additionalText = null) {
				WithActualOut(() => _er.InternalError(ex, additionalText));
				HasErrors = true;
			}
		}

		public CompilerDriver(IErrorReporter errorReporter) {
			_errorReporter = errorReporter;
		}

		private bool IsEntryPointCandidate(IMethod m) {
			if (m.Name != "Main" || !m.IsStatic || m.DeclaringTypeDefinition.TypeParameterCount > 0 || m.TypeParameters.Count > 0)	// Must be a static, non-generic Main
				return false;
			if (!m.ReturnType.IsKnownType(KnownTypeCode.Void) && !m.ReturnType.IsKnownType(KnownTypeCode.Int32))	// Must return void or int.
				return false;
			if (m.Parameters.Count == 0)	// Can have 0 parameters.
				return true;
			if (m.Parameters.Count > 1)	// May not have more than 1 parameter.
				return false;
			if (m.Parameters[0].IsRef || m.Parameters[0].IsOut)	// The single parameter must not be ref or out.
				return false;

			var at = m.Parameters[0].Type as ArrayType;
			return at != null && at.Dimensions == 1 && at.ElementType.IsKnownType(KnownTypeCode.String);	// The single parameter must be a one-dimensional array of strings.
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

		private static IEnumerable<IAssemblyResource> LoadResources(IEnumerable<EmbeddedResource> resources) {
			return resources.Select(r => new Resource(r.ResourceName, r.Filename, r.IsPublic));
		}

		private void InitializeAttributeStore(AttributeStore attributeStore, WindsorContainer container, ICompilation compilation) {
			var assemblies = compilation.Assemblies;
			var types = compilation.GetAllTypeDefinitions().ToList();
			foreach (var applier in container.ResolveAll<IAutomaticMetadataAttributeApplier>()) {
				foreach (var a in assemblies)
					applier.Process(a);
				foreach (var t in types)
					applier.Process(t);
			}
			attributeStore.RunAttributeCode();
		}

		public bool Compile(CompilerOptions options) {
			string intermediateAssemblyFile = Path.GetTempFileName(), intermediateDocFile = Path.GetTempFileName();
			var actualOut = Console.Out;
			var er = new ErrorReporterWrapper(_errorReporter, actualOut);
			try {
				Console.SetOut(new StringWriter());	// I don't trust the third-party libs to not generate spurious random messages, so make sure that any of those messages are suppressed.

				var settings = MapSettings(options, intermediateAssemblyFile, intermediateDocFile, er);
				if (er.HasErrors)
					return false;

				if (!options.AlreadyCompiled) {
					// Compile the assembly
					var ctx = new CompilerContext(settings, new ConvertingReportPrinter(er));
					var d = new Mono.CSharp.Driver(ctx);
					d.Compile();
					if (er.HasErrors)
						return false;
				}

				var references = LoadReferences(settings.AssemblyReferences, er);
				if (references == null)
					return false;

				PreparedCompilation compilation = PreparedCompilation.CreateCompilation(settings.AssemblyName, options.SourceFiles.Select(f => new SimpleSourceFile(f, settings.Encoding)), references.Select(r => r.Item1), options.DefineConstants, LoadResources(options.EmbeddedResources));

				IMethod entryPoint = FindEntryPoint(options, er, compilation);

				var container = new WindsorContainer();
				foreach (var plugin in TopologicalSortPlugins(references).Reverse())
					RegisterPlugin(container, plugin);

				var attributeStore = new AttributeStore(compilation.Compilation, er);

				container.Register(Component.For<IErrorReporter>().Instance(er),
				                   Component.For<CompilerOptions>().Instance(options),
				                   Component.For<IAttributeStore>().Instance(attributeStore),
				                   Component.For<ICompilation>().Instance(compilation.Compilation),
				                   Component.For<ICompiler>().ImplementedBy<Compiler.Compiler>()
				                  );

				InitializeAttributeStore(attributeStore, container, compilation.Compilation);

				container.Resolve<IMetadataImporter>().Prepare(compilation.Compilation.GetAllTypeDefinitions());

				var compiledTypes = container.Resolve<ICompiler>().Compile(compilation);

				foreach (var rewriter in container.ResolveAll<IJSTypeSystemRewriter>())
					compiledTypes = rewriter.Rewrite(compiledTypes);

				var invoker = new OOPEmulatorInvoker(container.Resolve<IOOPEmulator>(), container.Resolve<IMetadataImporter>(), container.Resolve<IErrorReporter>());

				var js = invoker.Process(compiledTypes.ToList(), entryPoint);
				js = container.Resolve<ILinker>().Process(js);

				if (er.HasErrors)
					return false;

				string outputAssemblyPath = !string.IsNullOrEmpty(options.OutputAssemblyPath) ? options.OutputAssemblyPath : Path.ChangeExtension(options.SourceFiles[0], ".dll");
				string outputScriptPath   = !string.IsNullOrEmpty(options.OutputScriptPath)   ? options.OutputScriptPath   : Path.ChangeExtension(options.SourceFiles[0], ".js");

				if (!options.AlreadyCompiled) {
					try {
						File.Copy(intermediateAssemblyFile, outputAssemblyPath, true);
					}
					catch (IOException ex) {
						er.Region = DomRegion.Empty;
						er.Message(Messages._7950, ex.Message);
						return false;
					}
					if (!string.IsNullOrEmpty(options.DocumentationFile)) {
						try {
							File.Copy(intermediateDocFile, options.DocumentationFile, true);
						}
						catch (IOException ex) {
							er.Region = DomRegion.Empty;
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
					er.Region = DomRegion.Empty;
					er.Message(Messages._7951, ex.Message);
					return false;
				}
				return true;
			}
			catch (Exception ex) {
				er.Region = DomRegion.Empty;
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

		private IMethod FindEntryPoint(CompilerOptions options, ErrorReporterWrapper er, PreparedCompilation compilation) {
			if (options.HasEntryPoint) {
				List<IMethod> candidates;
				if (!string.IsNullOrEmpty(options.EntryPointClass)) {
					var t = compilation.Compilation.MainAssembly.GetTypeDefinition(new FullTypeName(options.EntryPointClass));
					if (t == null) {
						er.Region = DomRegion.Empty;
						er.Message(Messages._7950, "Could not find the entry point class " + options.EntryPointClass + ".");
						return null;
					}
					candidates = t.Methods.Where(IsEntryPointCandidate).ToList();
				}
				else {
					candidates =
						compilation.Compilation.MainAssembly.GetAllTypeDefinitions()
						           .SelectMany(t => t.Methods)
						           .Where(IsEntryPointCandidate)
						           .ToList();
				}
				if (candidates.Count != 1) {
					er.Region = DomRegion.Empty;
					er.Message(Messages._7950, "Could not find a unique entry point.");
					return null;
				}
				return candidates[0];
			}

			return null;
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
					er.Region = DomRegion.Empty;
					foreach (var r in missingReferences)
						er.Message(Messages._7996, r);
					return null;
				}

				return assemblies.Select(asm => Tuple.Create(new IkvmLoader { IncludeInternalMembers = true }.LoadAssembly(asm), (IList<string>)GetReferencedAssemblyNames(asm).ToList(), LoadPlugin(asm))).ToList();
			}
		}
	}
}
