using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;
using TopologicalSort;

namespace Saltarelle.Compiler.OOPEmulation {
	public class OOPEmulatorInvoker {
		private readonly IOOPEmulator _emulator;
		private readonly IMetadataImporter _metadataImporter;
		private readonly IErrorReporter _errorReporter;

		public OOPEmulatorInvoker(IOOPEmulator emulator, IMetadataImporter metadataImporter, IErrorReporter errorReporter) {
			_emulator = emulator;
			_metadataImporter = metadataImporter;
			_errorReporter = errorReporter;
		}

		public IList<JsStatement> Process(IList<JsType> types, IMethodSymbol entryPoint) {
			var result = new List<JsStatement>();
			result.AddRange(_emulator.GetCodeBeforeFirstType(types));

			var processed = new List<Tuple<JsType, TypeOOPEmulation>>();
			foreach (var t in types) {
				try {
					processed.Add(Tuple.Create(t, _emulator.EmulateType(t)));
				}
				catch (Exception ex) {
					_errorReporter.Location = t.CSharpTypeDefinition.GetLocation();
					_errorReporter.InternalError(ex, "Error formatting type " + t.CSharpTypeDefinition.Name);
				}
			}

			if (processed.Count > 0) {
				int phases = processed.Max(x => x.Item2.Phases.Count);
				for (int i = 0; i < phases; i++) {
					var currentPhase = Order(processed.Select(x => Tuple.Create(x.Item1, (x.Item2.Phases.Count > i ? x.Item2.Phases[i] : null) ?? new TypeOOPEmulationPhase(null, null))).ToList());
					foreach (var c in currentPhase)
						result.AddRange(c.Item2.Statements);
				}
			}

			result.AddRange(_emulator.GetCodeAfterLastType(types));

			result.AddRange(GetStaticInitCode(types));

			if (entryPoint != null) {
				result.Add(InvokeEntryPoint(entryPoint));
			}

			return result;
		}

		private static Tuple<string, string> SplitIntoNamespaceAndName(string name) {
			int pos = name.LastIndexOf('.');
			if (pos == -1)
				return Tuple.Create("", name);
			else
				return Tuple.Create(name.Substring(0, pos), name.Substring(pos + 1));
		}

		internal static IEnumerable<T> OrderByNamespace<T>(IEnumerable<T> source, Func<T, string> nameSelector) {
			return    from s in source
			           let t = SplitIntoNamespaceAndName(nameSelector(s))
			       orderby t.Item1, t.Item2
			        select s;
		}

		private JsStatement InvokeEntryPoint(IMethodSymbol entryPoint) {
			if (entryPoint.Parameters.Length > 0) {
				_errorReporter.Location = entryPoint.GetLocation();
				_errorReporter.Message(Messages._7800, entryPoint.Name);
				return JsExpression.Null;
			}
			else {
				var sem = _metadataImporter.GetMethodSemantics(entryPoint);
				if (sem.Type != MethodScriptSemantics.ImplType.NormalMethod) {
					_errorReporter.Location = entryPoint.GetLocation();
					_errorReporter.Message(Messages._7801, entryPoint.Name);
					return JsExpression.Null;
				}
				else {
					return JsExpression.Invocation(JsExpression.Member(new JsTypeReferenceExpression(entryPoint.ContainingType), sem.Name));
				}
			}
		}

		private IEnumerable<JsStatement> GetStaticInitCode(IEnumerable<JsType> types) {
			return GetStaticInitializationOrder(OrderByNamespace(types.OfType<JsClass>(), c => _metadataImporter.GetTypeSemantics(c.CSharpTypeDefinition).Name), 1)
			       .SelectMany(_emulator.GetStaticInitStatements);
		}

		private IEnumerable<JsClass> GetStaticInitializationOrder(IEnumerable<JsClass> types, int pass) {
			if (pass > 3)
				return types;	// If we can't find a non-circular order after 3 passes, just use some random order.

			// We run the algorithm in 3 passes, each considering less types of references than the previous one.
			var dict = types.ToDictionary(t => t.CSharpTypeDefinition, t => new { deps = GetStaticInitializationDependencies(t, pass), backref = t });
			var edges = from s in dict from t in s.Value.deps where dict.ContainsKey(t) select Edge.Create(s.Key, t);

			var result = new List<JsClass>();
			foreach (var group in TopologicalSorter.FindAndTopologicallySortStronglyConnectedComponents(dict.Keys.ToList(), edges)) {
				var backrefed = group.Select(t => dict[t].backref);
				result.AddRange(group.Count > 1 ? GetStaticInitializationOrder(backrefed.ToList(), pass + 1) : backrefed);
			}

			return result;
		}

		private HashSet<INamedTypeSymbol> GetStaticInitializationDependencies(JsClass c, int pass) {
			// Consider the following reference locations:
			// Pass 1: static init statements, static methods, instance methods, constructors
			// Pass 2: static init statements, static methods
			// Pass 3: static init statements only

			var result = new HashSet<INamedTypeSymbol>();
			switch (pass) {
				case 1:
					foreach (var r in c.InstanceMethods.Where(m => m.Definition != null).SelectMany(m => TypeReferenceFinder.Analyze(m.Definition)))
						result.Add(r);
					foreach (var r in c.NamedConstructors.Where(m => m.Definition != null).SelectMany(m => TypeReferenceFinder.Analyze(m.Definition)))
						result.Add(r);
					if (c.UnnamedConstructor != null) {
						foreach (var r in TypeReferenceFinder.Analyze(c.UnnamedConstructor))
							result.Add(r);
					}
					goto case 2;

				case 2:
					foreach (var r in c.StaticMethods.Where(m => m.Definition != null).SelectMany(m => TypeReferenceFinder.Analyze(m.Definition)))
						result.Add(r);
					goto case 3;

				case 3:
					foreach (var r in TypeReferenceFinder.Analyze(c.StaticInitStatements))
						result.Add(r);
					break;

				default:
					throw new ArgumentException("pass");
			}
			return result;
		}

		private IEnumerable<Tuple<JsType, TypeOOPEmulationPhase>> Order(IList<Tuple<JsType, TypeOOPEmulationPhase>> source) {
			var backref = source.ToDictionary(x => x.Item1.CSharpTypeDefinition);
			var edges = from s in source from t in s.Item2.DependentOnTypes.Intersect(backref.Keys) select Edge.Create(s.Item1.CSharpTypeDefinition, t);
			var components = TopologicalSorter.FindAndTopologicallySortStronglyConnectedComponents(OrderByNamespace(backref.Keys, x => _metadataImporter.GetTypeSemantics(x).Name), edges);
			foreach (var error in components.Where(c => c.Count > 1)) {
				_errorReporter.Location = default(Location);
				_errorReporter.Message(Messages._7802, string.Join(", ", error.Select(t => t.MetadataName)));
			}
			return components.SelectMany(c => c).Select(t => backref[t]);
		}
	}
}
