using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.Roslyn;

namespace QUnit.Plugin {
	public class TestRewriter : IJSTypeSystemRewriter, IRuntimeContext {
		private readonly IErrorReporter _errorReporter;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly IAttributeStore _attributeStore;

		public TestRewriter(IErrorReporter errorReporter, IRuntimeLibrary runtimeLibrary, IAttributeStore attributeStore) {
			_errorReporter  = errorReporter;
			_runtimeLibrary = runtimeLibrary;
			_attributeStore = attributeStore;
		}

		private JsType ConvertType(JsClass type) {
			if (type.InstanceMethods.Any(m => m.Name == "runTests")) {
				_errorReporter.Location = type.CSharpTypeDefinition.Locations[0];
				_errorReporter.Message(DiagnosticSeverity.Error, "7019", string.Format("The type {0} cannot define a method named 'runTests' because it has a [TestFixtureAttribute].", type.CSharpTypeDefinition.FullyQualifiedName()));
				return type;
			}

			var instanceMethods = new List<JsMethod>();
			var tests = new List<Tuple<string, string, bool, int?, JsFunctionDefinitionExpression>>();

			foreach (var method in type.InstanceMethods) {
				var testAttr = _attributeStore.AttributesFor(method.CSharpMember).GetAttribute<TestAttribute>();
				if (testAttr != null) {
					if (method.CSharpMember.DeclaredAccessibility != Accessibility.Public || !((IMethodSymbol)method.CSharpMember).ReturnsVoid || ((IMethodSymbol)method.CSharpMember).Parameters.Length > 0 || ((IMethodSymbol)method.CSharpMember).TypeParameters.Length > 0) {
						_errorReporter.Location = method.CSharpMember.Locations[0];
						_errorReporter.Message(DiagnosticSeverity.Error, "7020", string.Format("Method {0}: Methods decorated with a [TestAttribute] must be public, non-generic, parameterless instance methods that return void.", method.CSharpMember.FullyQualifiedName()));
					}

					tests.Add(Tuple.Create(testAttr.Description ?? method.CSharpMember.Name, testAttr.Category, testAttr.IsAsync, testAttr.ExpectedAssertionCount >= 0 ? (int?)testAttr.ExpectedAssertionCount : null, method.Definition));
				}
				else
					instanceMethods.Add(method);
			}

			var testInvocations = new List<JsExpression>();

			foreach (var category in tests.GroupBy(t => t.Item2).Select(g => new { Category = g.Key, Tests = g.Select(x => new { Description = x.Item1, IsAsync = x.Item3, ExpectedAssertionCount = x.Item4, Function = x.Item5 }) }).OrderBy(x => x.Category)) {
				if (category.Category != null)
					testInvocations.Add(JsExpression.Invocation(JsExpression.Member(JsExpression.Identifier("QUnit"), "module"), JsExpression.String(category.Category)));
				testInvocations.AddRange(category.Tests.Select(t => JsExpression.Invocation(JsExpression.Identifier(t.IsAsync ? "asyncTest" : "test"), t.ExpectedAssertionCount != null ? new JsExpression[] { JsExpression.String(t.Description), JsExpression.Number(t.ExpectedAssertionCount.Value), _runtimeLibrary.Bind(t.Function, JsExpression.This, this) } : new JsExpression[] { JsExpression.String(t.Description), _runtimeLibrary.Bind(t.Function, JsExpression.This, this) })));
			}

			instanceMethods.Add(new JsMethod(null, "runTests", null, JsExpression.FunctionDefinition(new string[0], JsStatement.Block(testInvocations.Select(t => (JsStatement)t)))));

			var result = type.Clone();
			result.InstanceMethods.Clear();
			foreach (var m in instanceMethods)
				result.InstanceMethods.Add(m);
			return result;
		}

		public IEnumerable<JsType> Rewrite(IEnumerable<JsType> types) {
			foreach (var type in types) {
				var cls = type as JsClass;
				if (cls != null) {
					var attr = _attributeStore.AttributesFor(type.CSharpTypeDefinition).GetAttribute<TestFixtureAttribute>();
					yield return attr != null ? ConvertType(cls) : type;
				}
				else {
					yield return type;
				}
			}
		}

		public JsExpression ResolveTypeParameter(ITypeParameterSymbol tp) {
			throw new NotSupportedException();
		}

		public JsExpression EnsureCanBeEvaluatedMultipleTimes(JsExpression expression, IList<JsExpression> expressionsThatMustBeEvaluatedBefore) {
			throw new NotSupportedException();
		}
	}
}
