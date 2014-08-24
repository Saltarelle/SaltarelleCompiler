using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.Compiler {
	public class PreparedCompilation {
		[Obsolete("TODO: Move")]
		public static CSharpCompilation CreateCompilation(string assemblyName, OutputKind outputKind, IEnumerable<ISourceFile> sourceFiles, IEnumerable<MetadataReference> references, IList<string> defineConstants) {
			var defineConstantsArr = ImmutableArray.CreateRange(defineConstants ?? new string[0]);

			var syntaxTrees = sourceFiles.Select(f => { 
			                                        using (var rdr = f.Open()) {
			                                            return SyntaxFactory.ParseSyntaxTree(rdr.ReadToEnd(), new CSharpParseOptions(LanguageVersion.CSharp5, DocumentationMode.Diagnose, SourceCodeKind.Regular, defineConstantsArr), f.Filename);
			                                        }
			                                    }).ToList();

			return CSharpCompilation.Create(assemblyName, syntaxTrees, references, new CSharpCompilationOptions(outputKind));
		}
	}
}