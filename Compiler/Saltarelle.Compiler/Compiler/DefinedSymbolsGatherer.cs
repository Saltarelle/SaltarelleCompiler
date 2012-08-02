using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;

namespace Saltarelle.Compiler.Compiler {
	internal class DefinedSymbolsGatherer : DepthFirstAstVisitor {
		private readonly HashSet<string> _definedSymbols;

		private DefinedSymbolsGatherer(IEnumerable<string> definedSymbols) {
			_definedSymbols = new HashSet<string>(definedSymbols ?? new string[0]);
		}

		public override void VisitPreProcessorDirective(PreProcessorDirective preProcessorDirective) {
			if (preProcessorDirective.Type == PreProcessorDirectiveType.Define) {
				_definedSymbols.Add(preProcessorDirective.Argument);
			}
			else if (preProcessorDirective.Type == PreProcessorDirectiveType.Undef) {
				_definedSymbols.Remove(preProcessorDirective.Argument);
			}
			else {
				base.VisitPreProcessorDirective(preProcessorDirective);
			}
		}

		public static ISet<string> Gather(SyntaxTree syntaxTree, IEnumerable<string> predefinedSymbols) {
			var obj = new DefinedSymbolsGatherer(predefinedSymbols);
			syntaxTree.AcceptVisitor(obj);
			return obj._definedSymbols;
		}
	}
}
