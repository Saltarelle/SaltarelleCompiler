using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Options;
using Saltarelle.Compiler.Driver;

namespace Saltarelle.Compiler {
	public class Program {
		static readonly string _programName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);

		internal const string OptionsText =
@"    -debug              Preserve names in the generated script.
    -define:S1[;S2]     Defines one or more conditional symbols.
    -doc:FILE           Specifies output file for XML doc comments.
	-keycontainer:NAME  The key pair container used to sign the output assembly.
	-keyfile:FILE       The key file used to strongname the ouput assembly.
    -lib:PATH1[,PATHn]  Specifies locations to search for referenced assemblies.
    -nowarn:W1[,Wn]     Suppress one or more compiler warnings.
    -outasm:FILE        Specifies the output assembly file (default: base name of first file).
    -outscript:FILE     Specifies the output script file (default: base name of first file).
    -reference:A1[,An]  Imports metadata from the specified assemblies.
    -reference:ALIAS=A  Imports metadata using specified extern alias.
    -warn:0-4           Sets warning level, the default is 4.
    -warnaserror[+|-]:  Treats all warnings as errors.
    -warnaserror[+|-]:W1[,Wn]  Treats one or more compiler warnings as errors.
	-?  --help          Show this message";


		private static void HandleReferences(CompilerOptions options, string value) {
			foreach (var reference in value.Split(new[] { ',' }).Select(s => s.Trim()).Where(s => s != "")) {
				int i = reference.IndexOf('=');
				if (i >= 0)
					options.References.Add(new Reference(reference.Substring(i + 1), reference.Substring(0, i)));
				else
					options.References.Add(new Reference(reference));
			}
		}

		private static void HandleIntegerList(IList<int> targetCollection, string value, string optionName) {
			foreach (var s in value.Split(new[] { ',' }).Select(s => s.Trim()).Where(s => s != "")) {
				int w;
				if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out w))
					throw new OptionException("Could not convert string `" + s + "' to an integer for option `/" + optionName + "'.", "/" + optionName);
				if (!targetCollection.Contains(w))
					targetCollection.Add(w);
			}
		}

		private static void DisableWarnings(CompilerOptions options, string warnings) {
			HandleIntegerList(options.DisabledWarnings, warnings, "nowarn");
		}

		private static void HandleWarningsAsErrors(CompilerOptions options, string value) {
			string param;
			if (value != null && (value[0] == '-' || value[0] == '/')) {
				var colon = value.IndexOf(':');
				param = colon >= 0 ? value.Substring(colon + 1) : null;
			}
			else {
				param = value;
			}

			if (param != null) {
				HandleIntegerList(options.WarningsAsErrors, param, "warnaserror");
			}
			else {
				options.TreatWarningsAsErrors = true;
			}
		}

		private static void HandleWarningsNotAsErrors(CompilerOptions options, string value) {
			string param;
			if (value != null && (value[0] == '-' || value[0] == '/')) {
				var colon = value.IndexOf(':');
				param = colon >= 0 ? value.Substring(colon + 1) : null;
			}
			else {
				param = value;
			}

			if (param != null) {
				HandleIntegerList(options.WarningsNotAsErrors, param, "warnaserror-");
			}
			else {
				options.TreatWarningsAsErrors = false;
			}
		}

		private static void ShowHelp(TextWriter writer) {
			writer.WriteLine("Saltarelle C# to JavaScript compiler.");
			writer.WriteLine("For more information, see https://github.com/erik-kallen/SaltarelleCompiler/");
			writer.WriteLine("Usage: " + _programName + " [options] source-files");
			writer.WriteLine();
			writer.WriteLine("Options:");
			writer.WriteLine(OptionsText);
			writer.WriteLine();
			writer.WriteLine("Options can be of the form -option or /option");
		} 

		internal static CompilerOptions ParseOptions(string[] args, TextWriter infoWriter, TextWriter errorWriter) {
			if (args.Length == 0) {
				ShowHelp(infoWriter);
				return null;
			}

			try {
				bool showHelp = false;
				var result = new CompilerOptions() { MinimizeScript = true };
				var opts = new OptionSet {
					{ "outasm=",       v => result.OutputAssemblyPath = v },
					{ "outscript=",    v => result.OutputScriptPath = v },
					{ "doc=",          v => result.DocumentationFile = v },
					{ "define=",       v => result.DefineConstants.AddRange(v.Split(new[] { ';' }).Select(s => s.Trim()).Where(s => s != "" && !result.DefineConstants.Contains(s))) },
					{ "lib=",          v => result.AdditionalLibPaths.AddRange(v.Split(new[] { ',' }).Select(s => s.Trim()).Where(s => s != "" && !result.AdditionalLibPaths.Contains(s))) },
					{ "reference=",    v => HandleReferences(result, v) },
					{ "debug",         f => result.MinimizeScript = f == null || f.EndsWith("-") },
					{ "warn=",         (int v) => { if (v < 0 || v > 4) throw new OptionException("Warning level must be between 0 and 4", "/warn"); result.WarningLevel = v; } },
					{ "nowarn=",       v => DisableWarnings(result, v) },
					{ "warnaserror:",  v => HandleWarningsAsErrors(result, v) },
					{ "warnaserror-:", v => HandleWarningsNotAsErrors(result, v) },
					{ "keyfile=",      v => result.KeyFile = v },
					{ "keycontainer=", v => result.KeyContainer = v },
					{ "?|help",        v => showHelp = true },
				};

				var extra = opts.Parse(args);
				foreach (var file in extra)
					result.SourceFiles.Add(file);

				if (showHelp) {
					ShowHelp(infoWriter);
					return null;
				}

				return result;
			}
			catch (OptionException ex) {
				errorWriter.WriteLine(ex.Message);
				return null;
			}
		}

		static int Main(string[] args) {
			var options = ParseOptions(args, Console.Out, Console.Error);
			if (options != null) {
				var driver = new CompilerDriver(new ExecutableErrorReporter(Console.Out));
				bool result = driver.Compile(options, true);
				return result ? 0 : 1;
			}
			return 1;
		}
	}
}
