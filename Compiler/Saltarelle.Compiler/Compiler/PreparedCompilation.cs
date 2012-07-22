using System.Collections.Generic;
using System.Collections.ObjectModel;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.Compiler {
	public class PreparedCompilation {
		public ICompilation Compilation { get; private set; }

		internal class ParsedSourceFile {
			public CompilationUnit CompilationUnit { get; private set; }
			public CSharpParsedFile ParsedFile { get; private set; }

			public ParsedSourceFile(CompilationUnit compilationUnit, CSharpParsedFile parsedFile) {
				CompilationUnit = compilationUnit;
				ParsedFile      = parsedFile;
			}
		}

		internal ReadOnlyCollection<ParsedSourceFile> SourceFiles { get; private set; }

		internal PreparedCompilation(ICompilation compilation, IEnumerable<ParsedSourceFile> sourceFiles) {
			Compilation = compilation;
			SourceFiles = sourceFiles.AsReadOnly();
		}
	}
}