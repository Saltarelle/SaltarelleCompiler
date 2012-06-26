using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Policy;
using System.Text;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.CSharp;
using System.Linq;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.ReferenceImporter;
using Saltarelle.Compiler.RuntimeLibrary;

namespace Saltarelle.Compiler.Driver {
	public class CompilerDriver {
		private readonly IErrorReporter _errorReporter2;

		private string GetAssemblyName(CompilerOptions options) {
			if (options.OutputAssemblyPath != null)
				return Path.GetFileNameWithoutExtension(options.OutputAssemblyPath);
			else if (options.SourceFiles.Count > 0)
				return Path.GetFileNameWithoutExtension(options.SourceFiles[0]);
			else
				return null;
		}

		private string ResolveReference(string filename, IEnumerable<string> paths, IErrorReporter er) {
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
			er.Message(7997, null, TextLocation.Empty, filename);
			return null;
		}

		private CompilerSettings MapSettings(CompilerOptions options, string outputAssemblyPath, string outputDocFilePath, IErrorReporter er) {
			var allPaths = options.AdditionalLibPaths.Concat(new[] { Environment.CurrentDirectory }).ToList();

			var result = new CompilerSettings();
			result.Target                    = Target.Library;
			result.Platform                  = Platform.AnyCPU;
			result.TargetExt                 = ".dll";
			result.VerifyClsCompliance       = false;
			result.Optimize                  = false;
			result.Version                   = LanguageVersion.V_5;
			result.EnhancedWarnings          = false;
			result.LoadDefaultReferences     = false;
			result.TabSize                   = 1;
			result.WarningsAreErrors         = options.TreatWarningsAsErrors;
			result.FatalCounter              = 100;
			result.WarningLevel              = options.WarningLevel;
			result.AssemblyReferences        = options.References.Where(r => r.Alias == null).Select(r => ResolveReference(r.Filename, allPaths, er)).ToList();
			result.AssemblyReferencesAliases = options.References.Where(r => r.Alias != null).Select(r => new Mono.CSharp.Tuple<string, string>(r.Alias, ResolveReference(r.Filename, allPaths, er))).ToList();
			result.Encoding                  = Encoding.UTF8;
			result.DocumentationFile         = !string.IsNullOrEmpty(options.DocumentationFile) ? outputDocFilePath : null;
			result.OutputFile                = outputAssemblyPath;
			result.AssemblyName              = GetAssemblyName(options);
			result.StdLib                    = false;
			result.StdLibRuntimeVersion      = RuntimeVersion.v4;
			result.StrongNameKeyContainer    = options.KeyContainer;
			result.StrongNameKeyFile         = options.KeyFile;
			result.SourceFiles.AddRange(options.SourceFiles.Select((f, i) => new SourceFile(f, f, i + 1)));
			foreach (var c in options.DefineConstants)
				result.AddConditionalSymbol(c);
			foreach (var w in options.DisabledWarnings)
				result.SetIgnoreWarning(w);
			result.SetIgnoreWarning(660);	// 660 and 661: class defines operator == or operator != but does not override Equals / GetHashCode. These warnings don't really apply, since we have no Equals / GetHashCode methods to override.
			result.SetIgnoreWarning(661);
			foreach (var w in options.WarningsAsErrors)
				result.AddWarningAsError(w);
			foreach (var w in options.WarningsNotAsErrors)
				result.AddWarningOnly(w);

			if (result.AssemblyReferencesAliases.Count > 0)	// NRefactory does currently not support reference aliases, this check will hopefully go away in the future.
				er.Message(7998, null, TextLocation.Empty, "aliased reference");

			return result;
		}

		private class ConvertingReportPrinter : ReportPrinter {
			private readonly IErrorReporter _errorReporter;

			public ConvertingReportPrinter(IErrorReporter errorReporter) {
				_errorReporter = errorReporter;
			}

			public override void Print(AbstractMessage msg, bool showFullPath) {
				base.Print(msg, showFullPath);
				_errorReporter.Message(msg.IsWarning ? MessageSeverity.Warning : MessageSeverity.Error, msg.Code, msg.Location.NameFullPath, new TextLocation(msg.Location.Row, msg.Location.Column), msg.Text.Replace("{", "{{").Replace("}", "}}"));
			}
		}

		private class SimpleSourceFile : ISourceFile {
			private readonly string _filename;

			public SimpleSourceFile(string filename) {
				_filename = filename;
			}

			public string FileName {
				get { return _filename; }
			}

			public TextReader Open() {
				return new StreamReader(FileName);
			}
		}

		private class ErrorReporterWrapper : IErrorReporter {
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

			public void Message(MessageSeverity severity, int code, string file, TextLocation location, string message, params object[] args) {
				WithActualOut(() => _er.Message(severity, code, file, location, message, args));
				if (severity == MessageSeverity.Error)
					HasErrors = true;
			}

			public void InternalError(string text, string file, TextLocation location) {
				WithActualOut(() => _er.InternalError(text, file, location));
				HasErrors = true;
			}

			public void InternalError(Exception ex, string file, TextLocation location, string additionalText = null) {
				WithActualOut(() => _er.InternalError(ex, file, location, additionalText));
				HasErrors = true;
			}
		}

		public CompilerDriver(IErrorReporter errorReporter) {
			_errorReporter2 = errorReporter;
		}

		public bool Compile(CompilerOptions options) {
			try {
				string intermediateAssemblyFile = Path.GetTempFileName(), intermediateDocFile = Path.GetTempFileName();
				var actualOut = Console.Out;
				try {
					Console.SetOut(new StringWriter());	// I don't trust the third-party libs to not generate spurious random messages, so make sure that any of those messages are suppressed.

					var er = new ErrorReporterWrapper(_errorReporter2, actualOut);
					// Compile the assembly
					var settings = MapSettings(options, intermediateAssemblyFile, intermediateDocFile, er);
					if (er.HasErrors)
						return false;

					var ctx = new CompilerContext(settings, new ConvertingReportPrinter(er));
					var d = new Mono.CSharp.Driver(ctx);
					d.Compile();

					if (er.HasErrors)
						return false;

					// Compile the script
					var nc = new MetadataImporter.ScriptSharpMetadataImporter(options.MinimizeScript);
					PreparedCompilation compilation = null;
					var rtl = new ScriptSharpRuntimeLibrary(nc, tr => Utils.CreateJsTypeReferenceExpression(tr.Resolve(compilation.Compilation).GetDefinition(), nc));
					var compiler = new Saltarelle.Compiler.Compiler.Compiler(nc, rtl, er);

					compilation = compiler.CreateCompilation(options.SourceFiles.Select(f => new SimpleSourceFile(f)), LoadReferences(ctx.Settings.AssemblyReferences), options.DefineConstants);
					var compiledTypes = compiler.Compile(compilation);

					var js = new OOPEmulator.ScriptSharpOOPEmulator(nc, er).Rewrite(compiledTypes, compilation.Compilation);
					js = new GlobalNamespaceReferenceImporter().ImportReferences(js);

					if (er.HasErrors)
						return false;

					var asm = Mono.Cecil.AssemblyDefinition.ReadAssembly(intermediateAssemblyFile);
					// TODO: Metadata writeback.

					string outputAssemblyPath = !string.IsNullOrEmpty(options.OutputAssemblyPath) ? options.OutputAssemblyPath : Path.ChangeExtension(options.SourceFiles[0], ".dll");
					string outputScriptPath   = !string.IsNullOrEmpty(options.OutputScriptPath)   ? options.OutputScriptPath   : Path.ChangeExtension(options.SourceFiles[0], ".js");

					try {
						asm.Write(outputAssemblyPath);
					}
					catch (IOException ex) {
						er.Message(7950, null, TextLocation.Empty, ex.Message);
						return false;
					}
					if (!string.IsNullOrEmpty(options.DocumentationFile)) {
						try {
							File.Copy(intermediateDocFile, options.DocumentationFile, true);
						}
						catch (IOException ex) {
							er.Message(7952, null, TextLocation.Empty, ex.Message);
							return false;
						}
					}

					string script = string.Join("", js.Select(s => OutputFormatter.Format(s, allowIntermediates: false)));
					try {
						File.WriteAllText(outputScriptPath, script);
					}
					catch (IOException ex) {
						er.Message(7951, null, TextLocation.Empty, ex.Message);
						return false;
					}
				}
				finally {
					try { File.Delete(intermediateAssemblyFile); } catch {}
					try { File.Delete(intermediateDocFile); } catch {}
					Console.SetOut(actualOut);
				}
			}
			catch (Exception ex) {
				_errorReporter2.InternalError(ex, null, TextLocation.Empty);
			}

			return true;
		}

		private IList<IAssemblyReference> LoadReferences(IEnumerable<string> references) {
			// Shouldn't result in errors because mcs would have caught it.
			var loader = new CecilLoader { IncludeInternalMembers = true };
			return references.Select(f => (IAssemblyReference)loader.LoadAssemblyFile(f)).ToList();
		}
	}
}
