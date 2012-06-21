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
		private readonly IErrorReporter _errorReporter;

		private CompilerSettings MapSettings(CompilerOptions options, string outputAssemblyPath) {
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
			result.AssemblyReferences        = options.References.Where(r => r.Alias == null).Select(r => r.Assembly).ToList();
			result.AssemblyReferencesAliases = options.References.Where(r => r.Alias != null).Select(r => new Mono.CSharp.Tuple<string, string>(r.Alias, r.Assembly)).ToList();
			result.ReferencesLookupPaths     = options.AdditionalLibPaths;
			result.Encoding                  = Encoding.UTF8;
			result.DocumentationFile         = options.DocumentationFile;
			result.OutputFile                = outputAssemblyPath;
			result.StdLib                    = false;
			result.StdLibRuntimeVersion      = RuntimeVersion.v4;
			result.SourceFiles.AddRange(options.SourceFiles.Select((f, i) => new SourceFile(Path.GetFileName(f), f, i + 1)));
			foreach (var c in options.DefineConstants)
				result.AddConditionalSymbol(c);
			foreach (var w in options.DisabledWarnings)
				result.SetIgnoreWarning(w);
			foreach (var w in options.WarningsAsErrors)
				result.AddWarningAsError(w);
			foreach (var w in options.WarningsNotAsErrors)
				result.AddWarningOnly(w);
			foreach (var w in options.IgnoredWarnings)
				result.SetIgnoreWarning(w);

			return result;
		}

		class ConvertingReportPrinter : ReportPrinter {
			private readonly IErrorReporter _errorReporter;

			public ConvertingReportPrinter(IErrorReporter errorReporter) {
				_errorReporter = errorReporter;
			}

			public override void Print(AbstractMessage msg, bool showFullPath) {
				base.Print(msg, showFullPath);
				_errorReporter.Message(msg.IsWarning ? MessageSeverity.Warning : MessageSeverity.Error, msg.Code, msg.Location.NameFullPath, new TextLocation(msg.Location.Row, msg.Location.Column), msg.Text.Replace("{", "{{").Replace("}", "}}"));
			}
		}

		class SimpleSourceFile : ISourceFile {
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

		class ErrorReporterWrapper : IErrorReporter {
			private readonly IErrorReporter _er;

			public bool HasErrors { get; private set; }

			public ErrorReporterWrapper(IErrorReporter er) {
				_er = er;
			}

			public void Message(MessageSeverity severity, int code, string file, TextLocation location, string message, params object[] args) {
				_er.Message(severity, code, file, location, message, args);
				if (severity == MessageSeverity.Error)
					HasErrors = true;
			}

			public void InternalError(string text, string file, TextLocation location) {
				_er.InternalError(text, file, location);
				HasErrors = true;
			}

			public void InternalError(Exception ex, string file, TextLocation location, string additionalText = null) {
				_er.InternalError(ex, file, location, additionalText);
				HasErrors = true;
			}
		}

		public CompilerDriver(IErrorReporter errorReporter) {
			_errorReporter = errorReporter;
		}

		public bool Compile(CompilerOptions options) {
			string intermediateAssemblyFile = Path.GetTempFileName();

			try {
				// TODO: extern alias not supported.

				var er = new ErrorReporterWrapper(_errorReporter);
				// Compile the assembly
				var settings = MapSettings(options, intermediateAssemblyFile);
				var ctx = new CompilerContext(settings, new ConvertingReportPrinter(er));
				var d = new Mono.CSharp.Driver(ctx);
				d.Compile();

				if (er.HasErrors)
					return false;

				// Compile the script
				var nc = new MetadataImporter.ScriptSharpMetadataImporter(options.MinimizeNames);
				PreparedCompilation compilation = null;
				var rtl = new ScriptSharpRuntimeLibrary(nc, tr => Utils.CreateJsTypeReferenceExpression(tr.Resolve(compilation.Compilation).GetDefinition(), nc));
				var compiler = new Saltarelle.Compiler.Compiler.Compiler(nc, rtl, _errorReporter);

				var refs = LoadReferences(options.References.Select(r => r.Assembly), options.AdditionalLibPaths, er);
				if (er.HasErrors)
					return false;

				compilation = compiler.CreateCompilation(options.SourceFiles.Select(f => new SimpleSourceFile(f)), refs, options.DefineConstants);
				var compiledTypes = compiler.Compile(compilation);

				var js = new OOPEmulator.ScriptSharpOOPEmulator(nc, er).Rewrite(compiledTypes, compilation.Compilation);
				js = new GlobalNamespaceReferenceImporter().ImportReferences(js);

				if (er.HasErrors)
					return false;

				var asm = Mono.Cecil.AssemblyDefinition.ReadAssembly(intermediateAssemblyFile);
				// TODO: Metadata writeback.
				asm.Write(options.OutputAssemblyPath);

				string script = string.Join("", js.Select(s => OutputFormatter.Format(s, allowIntermediates: false)));
				File.WriteAllText(options.OutputScriptPath, script);
			}
			finally {
				try {
					File.Delete(intermediateAssemblyFile);
				}
				catch {}
			}

			return true;
		}

		private IList<IAssemblyReference> LoadReferences(IEnumerable<string> references, List<string> additionalLibPaths, IErrorReporter er) {
			// TODO: Error handling, actually use the additional lib paths.

			var loader = new CecilLoader { IncludeInternalMembers = true };
			var result = new List<IAssemblyReference>();
			foreach (var reference in references) {
				result.Add(loader.LoadAssemblyFile(reference));
			}
			
			return result;
		}
	}
}
