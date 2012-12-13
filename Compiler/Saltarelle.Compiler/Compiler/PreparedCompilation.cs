using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.Compiler {
	public class PreparedCompilation {
		public ICompilation Compilation { get; private set; }

		internal class ParsedSourceFile {
			public SyntaxTree SyntaxTree { get; private set; }
			public CSharpUnresolvedFile ParsedFile { get; private set; }
			public ISet<string> DefinedSymbols { get; private set; }

			public ParsedSourceFile(SyntaxTree syntaxTree, CSharpUnresolvedFile parsedFile, ISet<string> definedSymbols) {
				SyntaxTree     = syntaxTree;
				ParsedFile     = parsedFile;
				DefinedSymbols = definedSymbols;
			}
		}

		internal ReadOnlyCollection<ParsedSourceFile> SourceFiles { get; private set; }

		private PreparedCompilation(ICompilation compilation, IEnumerable<ParsedSourceFile> sourceFiles) {
			Compilation = compilation;
			SourceFiles = sourceFiles.AsReadOnly();
		}

		private static CSharpParser CreateParser(IEnumerable<string> defineConstants) {
			var parser = new CSharpParser();
			if (defineConstants != null) {
				foreach (var c in defineConstants)
					parser.CompilerSettings.ConditionalSymbols.Add(c);
			}
			return parser;
		}

		public static PreparedCompilation CreateCompilation(IEnumerable<ISourceFile> sourceFiles, IEnumerable<IAssemblyReference> references, IList<string> defineConstants) {
            IProjectContent project = new CSharpProjectContent();

            var files = sourceFiles.Select(f => { 
                                                    using (var rdr = f.Open()) {
                                                        var syntaxTree = CreateParser(defineConstants).Parse(rdr, f.Filename);
                                                        var expandResult = new QueryExpressionExpander().ExpandQueryExpressions(syntaxTree);
                                                        syntaxTree = (expandResult != null ? (SyntaxTree)expandResult.AstNode : syntaxTree);
                                                        var definedSymbols = DefinedSymbolsGatherer.Gather(syntaxTree, defineConstants);
                                                        return new PreparedCompilation.ParsedSourceFile(syntaxTree, new CSharpUnresolvedFile(f.Filename, new UsingScope()), definedSymbols);
                                                    }
                                                }).ToList();

            foreach (var f in files) {
                var tcv = new TypeSystemConvertVisitor(f.ParsedFile);
                f.SyntaxTree.AcceptVisitor(tcv);
                project = project.AddOrUpdateFiles(f.ParsedFile);
            }
            project = project.AddAssemblyReferences(references);

            return new PreparedCompilation(project.CreateCompilation(), files);
		}
	}
}