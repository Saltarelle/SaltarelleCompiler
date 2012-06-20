using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Policy;
using System.Text;
using Mono.CSharp;
using System.Linq;

namespace Saltarelle.Compiler.Driver {
	public class CompilerDriver {
		private CompilerSettings MapSettings(CompilerOptions options) {
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
			result.WarningLevel              = options.WarningLevel;
			result.AssemblyReferences        = options.References.Where(r => r.Alias == null).Select(r => r.File).ToList();
			result.AssemblyReferencesAliases = options.References.Where(r => r.Alias != null).Select(r => new Mono.CSharp.Tuple<string, string>(r.Alias, r.File)).ToList();
			result.ReferencesLookupPaths     = options.AdditionalLibPaths;
			result.Encoding                  = Encoding.UTF8;
			result.DocumentationFile         = options.DocumentationFile;
			result.OutputFile                = options.OutputAssemblyPath;
			result.StdLib                    = false;
			result.StdLibRuntimeVersion      = RuntimeVersion.v4;
			result.SourceFiles.AddRange(options.SourceFiles.Select((f, i) => new SourceFile(Path.GetFileName(f), f, i)));
			foreach (var c in options.DefineConstants)
				result.AddConditionalSymbol(c);
			foreach (var w in options.WarningsAsErrors)
				result.AddWarningAsError(w);
			foreach (var w in options.WarningsNotAsErrors)
				result.AddWarningOnly(w);
			foreach (var w in options.IgnoredWarnings)
				result.SetIgnoreWarning(w);

			return result;
		}

		public void Compile(CompilerOptions options) {
			var wr = new StringWriter();

			var rpt = new StreamReportPrinter(wr);
			var settings = MapSettings(options);
			var ctx = new CompilerContext(settings, rpt);
			var d = new Mono.CSharp.Driver(ctx);
			d.Compile();
		}
	}
}
