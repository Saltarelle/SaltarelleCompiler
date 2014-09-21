using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Roslyn;

namespace Saltarelle.Compiler.Compiler.Expressions {
	public partial class ExpressionCompiler : CSharpSyntaxVisitor<JsExpression>, IRuntimeContext {
		private readonly Compilation _compilation;
		private readonly SemanticModel _semanticModel;
		private readonly IMetadataImporter _metadataImporter;
		private readonly INamer _namer;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly IErrorReporter _errorReporter;
		private readonly IDictionary<ISymbol, VariableData> _variables;
		private readonly Func<ILocalSymbol> _createTemporaryVariable;
		private readonly Func<NestedFunctionContext, ImmutableDictionary<IRangeVariableSymbol, JsExpression>, StatementCompiler> _createInnerCompiler;
		private readonly string _thisAlias;
		private readonly NestedFunctionContext _nestedFunctionContext;
		#warning cache must be reset for each compiled expression
		private readonly Dictionary<ITypeSymbol, JsIdentifierExpression> _anonymousAndTransparentTypeCache;
		private ImmutableDictionary<IRangeVariableSymbol, JsExpression> _activeRangeVariableSubstitutions;
		private bool _returnMultidimArrayValueByReference;
		private bool _returnValueIsImportant;
		private bool _ignoreConversion;
		private List<JsStatement> _additionalStatements;

		public ExpressionCompiler(Compilation compilation, SemanticModel semanticModel, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, IDictionary<ISymbol, VariableData> variables, Func<ILocalSymbol> createTemporaryVariable, Func<NestedFunctionContext, ImmutableDictionary<IRangeVariableSymbol, JsExpression>, StatementCompiler> createInnerCompiler, string thisAlias, NestedFunctionContext nestedFunctionContext, ImmutableDictionary<IRangeVariableSymbol, JsExpression> activeRangeVariableSubstitutions, Dictionary<ITypeSymbol, JsIdentifierExpression> anonymousAndTransparentTypeCache) {
			Require.ValidJavaScriptIdentifier(thisAlias, "thisAlias", allowNull: true);

			_compilation = compilation;
			_semanticModel = semanticModel;
			_metadataImporter = metadataImporter;
			_namer = namer;
			_runtimeLibrary = runtimeLibrary;
			_errorReporter = errorReporter;
			_variables = variables;
			_createTemporaryVariable = createTemporaryVariable;
			_createInnerCompiler = createInnerCompiler;
			_thisAlias = thisAlias;
			_nestedFunctionContext = nestedFunctionContext;
			_activeRangeVariableSubstitutions = activeRangeVariableSubstitutions;
			_anonymousAndTransparentTypeCache = anonymousAndTransparentTypeCache;
			_returnMultidimArrayValueByReference = false;
		}

		public ExpressionCompileResult Compile(ExpressionSyntax expression, bool returnValueIsImportant, bool ignoreConversion = false, bool returnMultidimArrayValueByReference = false) {
			_additionalStatements = new List<JsStatement>();
			_ignoreConversion = ignoreConversion;
			var result = Visit(expression, returnValueIsImportant, returnMultidimArrayValueByReference);
			return new ExpressionCompileResult(result, _additionalStatements);
		}

		public ExpressionCompileResult CompileMethodCall(ExpressionSyntax target, IEnumerable<ExpressionSyntax> arguments, IMethodSymbol method, bool returnValueIsImportant) {
			_additionalStatements = new List<JsStatement>();
			_returnValueIsImportant = returnValueIsImportant;
			_returnMultidimArrayValueByReference = false;
			var sem = _metadataImporter.GetMethodSemantics(method);
			var result = CompileMethodInvocation(sem, method, usedMultipleTimes => InnerCompile(target, usedMultipleTimes, returnMultidimArrayValueByReference: true), false, ArgumentMap.CreateIdentity(arguments), false);
			return new ExpressionCompileResult(result, _additionalStatements);
		}

		public ExpressionCompileResult CompileMethodCall(JsExpression target, IEnumerable<ExpressionSyntax> arguments, IMethodSymbol method, bool returnValueIsImportant) {
			_additionalStatements = new List<JsStatement>();
			_returnValueIsImportant = returnValueIsImportant;
			_returnMultidimArrayValueByReference = false;
			var sem = _metadataImporter.GetMethodSemantics(method);
			var result = CompileMethodInvocation(sem, method, _ => target, false, ArgumentMap.CreateIdentity(arguments), false);
			return new ExpressionCompileResult(result, _additionalStatements);
		}

		public ExpressionCompileResult CompileMethodCall(JsExpression target, IEnumerable<JsExpression> arguments, IMethodSymbol method, bool returnValueIsImportant) {
			_additionalStatements = new List<JsStatement>();
			_returnValueIsImportant = returnValueIsImportant;
			_returnMultidimArrayValueByReference = false;
			var sem = _metadataImporter.GetMethodSemantics(method);
			var result = CompileMethodInvocation(sem, method, new[] { target }.Concat(arguments).ToList(), false);
			return new ExpressionCompileResult(result, _additionalStatements);
		}

		public ExpressionCompileResult CompileObjectConstruction(IList<JsExpression> arguments, IMethodSymbol constructor) {
			_additionalStatements = new List<JsStatement>();
			_returnValueIsImportant = true;
			_returnMultidimArrayValueByReference = false;

			var sem = _metadataImporter.GetConstructorSemantics(constructor);
			JsExpression expression = null;
			if (sem.Type == ConstructorScriptSemantics.ImplType.Json) {
				var properties = new List<JsObjectLiteralProperty>();
				for (int i = 0; i < arguments.Count; i++) {
					var m = sem.ParameterToMemberMap[i];
					string name = GetMemberNameForJsonConstructor(m);
					if (name != null) {
						properties.Add(new JsObjectLiteralProperty(name, arguments[i]));
					}
				}
				expression = JsExpression.ObjectLiteral(properties);
			}
			else {
				expression = CompileNonJsonConstructorInvocation(sem, constructor, arguments, false);
			}

			return new ExpressionCompileResult(expression, _additionalStatements);
		}

		public ExpressionCompileResult CompileAnonymousObjectConstruction(INamedTypeSymbol anonymousType, IEnumerable<Tuple<ISymbol, JsExpression>> initializers) {
			_additionalStatements = new List<JsStatement>();
			_returnValueIsImportant = true;
			_returnMultidimArrayValueByReference = false;

			var properties = new List<JsObjectLiteralProperty>();

			foreach (var init in initializers) {
				if (init.Item1 != null) {
					string name = GetMemberNameForJsonConstructor(init.Item1);
					if (name != null) {
						properties.Add(new JsObjectLiteralProperty(name, init.Item2));
					}
				}
				else {
					_errorReporter.InternalError("Expected an assignment to an identifier, got " + init.Item2);
				}
			}

			return new ExpressionCompileResult(JsExpression.ObjectLiteral(properties), _additionalStatements);
		}

		public ExpressionCompileResult CompilePropertyRead(JsExpression target, IPropertySymbol property) {
			_additionalStatements = new List<JsStatement>();
			_returnValueIsImportant = true;
			_returnMultidimArrayValueByReference = false;
			var result = HandleMemberRead(_ => target, property, false, false);
			return new ExpressionCompileResult(result, _additionalStatements);
		}

		public ExpressionCompileResult CompileConversion(JsExpression target, ITypeSymbol fromType, ITypeSymbol toType) {
			_additionalStatements = new List<JsStatement>();
			_returnValueIsImportant = true;
			_returnMultidimArrayValueByReference = false;
			var result = PerformConversion(target, _compilation.ClassifyConversion(fromType, toType), fromType, toType, null);
			return new ExpressionCompileResult(result, _additionalStatements);
		}

		public JsExpression CompileDelegateCombineCall(JsExpression a, JsExpression b) {
			var del = _compilation.GetSpecialType(SpecialType.System_Delegate);
			var combine = (IMethodSymbol)del.GetMembers("Combine").Single();
			var impl = _metadataImporter.GetMethodSemantics(combine);
			var thisAndArguments = (combine.IsStatic ? new[] { InstantiateType(del), a, b } : new[] { a, b });
			return CompileMethodInvocation(impl, combine, thisAndArguments, false);
		}

		public JsExpression CompileDelegateRemoveCall(JsExpression a, JsExpression b) {
			var del = _compilation.GetSpecialType(SpecialType.System_Delegate);
			var remove = (IMethodSymbol)del.GetMembers("Remove").Single();
			var impl = _metadataImporter.GetMethodSemantics(remove);
			var thisAndArguments = (remove.IsStatic ? new[] { InstantiateType(del), a, b } : new[] { a, b });
			return CompileMethodInvocation(impl, remove, thisAndArguments, false);
		}

		public IList<JsStatement> CompileConstructorInitializer(IMethodSymbol method, ArgumentMap argumentMap, bool currentIsStaticMethod) {
			var impl = _metadataImporter.GetConstructorSemantics(method);
			if (impl.SkipInInitializer) {
				if (currentIsStaticMethod)
					return new[] { JsStatement.Var(_thisAlias, JsExpression.ObjectLiteral()) };
				else
					return new JsStatement[0];
			}

			_additionalStatements = new List<JsStatement>();
			_returnValueIsImportant = false;
			_returnMultidimArrayValueByReference = false;

			if (currentIsStaticMethod) {
				_additionalStatements.Add(JsStatement.Var(_thisAlias, CompileConstructorInvocation(impl, method, argumentMap, ImmutableArray<Tuple<ISymbol, ExpressionSyntax>>.Empty)));
			}
			else if (impl.Type == ConstructorScriptSemantics.ImplType.Json) {
				_additionalStatements.Add(_runtimeLibrary.ShallowCopy(CompileJsonConstructorCall(impl, argumentMap, ImmutableArray<Tuple<ISymbol, ExpressionSyntax>>.Empty), CompileThis(), this));
			}
			else {
				string literalCode   = GetActualInlineCode(impl, argumentMap.CanBeTreatedAsExpandedForm);
				var thisAndArguments = CompileThisAndArgumentListForMethodCall(method, literalCode, InstantiateType(method.ContainingType), false, argumentMap);
				var jsType           = thisAndArguments[0];
				thisAndArguments[0]  = CompileThis();	// Swap out the TypeResolveResult that we get as default.
			
				switch (impl.Type) {
					case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
						_additionalStatements.Add(CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, jsType, impl.ExpandParams, true));
						break;
			
					case ConstructorScriptSemantics.ImplType.NamedConstructor:
						_additionalStatements.Add(CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, JsExpression.Member(jsType, impl.Name), impl.ExpandParams, true));
						break;
			
					case ConstructorScriptSemantics.ImplType.StaticMethod:
						_additionalStatements.Add(_runtimeLibrary.ShallowCopy(CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, JsExpression.Member(jsType, impl.Name), impl.ExpandParams, false), thisAndArguments[0], this));
						break;
			
					case ConstructorScriptSemantics.ImplType.InlineCode:
						_additionalStatements.Add(_runtimeLibrary.ShallowCopy(CompileInlineCodeMethodInvocation(method, literalCode, null, thisAndArguments.Skip(1).ToList()), thisAndArguments[0], this));
						break;
			
					default:
						_errorReporter.Message(Messages._7505);
						break;
				}
			}

			var result = _additionalStatements;
			_additionalStatements = null;	// Just so noone else messes with it by accident (shouldn't happen).
			return result;
		}

		public ExpressionCompileResult CompileAttributeConstruction(AttributeData attribute) {
			try {
				_additionalStatements = new List<JsStatement>();
				_returnValueIsImportant = true;
				_returnMultidimArrayValueByReference = false;

				var sem = _metadataImporter.GetConstructorSemantics(attribute.AttributeConstructor);
				var result = CompileConstructorInvocation(sem, attribute.AttributeConstructor, attribute.GetConstructorArgumentMap(), ImmutableArray<Tuple<ISymbol, ExpressionSyntax>>.Empty);
				if (attribute.NamedArguments.Length > 0) {
					var target = _createTemporaryVariable();
					var targetName = _variables[target].Name;
					_additionalStatements.Add(JsStatement.Var(targetName, result));
					result = JsExpression.Identifier(targetName);
					foreach (var init in attribute.GetNamedArgumentMap()) {
						var jsInit = CompileMemberAssignment(_ => result, false, init.Item2.Item1, init.Item1, null, new ArgumentForCall(init.Item2), null, (a, b) => b, returnValueIsImportant: false, returnValueBeforeChange: false, oldValueIsImportant: false);
						if (jsInit.NodeType != ExpressionNodeType.Null)
							_additionalStatements.Add(jsInit);
					}
				}
				return new ExpressionCompileResult(result, _additionalStatements);
			}
			catch (Exception ex) {
				_errorReporter.Location = attribute.ApplicationSyntaxReference.GetSyntax().GetLocation();
				_errorReporter.InternalError(ex);
				return new ExpressionCompileResult(JsExpression.Null, _additionalStatements);
			}
		}

		JsExpression IRuntimeContext.ResolveTypeParameter(ITypeParameterSymbol tp) {
			return ResolveTypeParameter(tp);
		}

		JsExpression IRuntimeContext.EnsureCanBeEvaluatedMultipleTimes(JsExpression expression, IList<JsExpression> expressionsThatMustBeEvaluatedBefore) {
			return Utils.EnsureCanBeEvaluatedMultipleTimes(_additionalStatements, expression, expressionsThatMustBeEvaluatedBefore, () => { var temp = _createTemporaryVariable(); return _variables[temp].Name; });
		}

		private JsExpression ResolveTypeParameter(ITypeParameterSymbol tp) {
			return Utils.ResolveTypeParameter(tp, _metadataImporter, _errorReporter, _namer);
		}
	}
}
